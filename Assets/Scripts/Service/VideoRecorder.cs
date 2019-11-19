#if UNITY_EDITOR || UNITY_STANDALONE
#  define STANDALONE_RECORDER
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RockVR.Video;
using UnityEngine;
using System.IO;

public class VideoRecorder
{
    public static event Action onStartRecording;
    public static event Action onStopRecording;

    private const int s_kMinimumRecordingSeconds = 2;

    public enum State
    {
        Stopped,
        Started,
        Stopping,
        CopyToAlbum
    }

    // sdk specific recorder object
    private static GameObject s_recorderImpl;
    // per user video directory
    private static string s_videoCacheDir;
    private static Action s_onStoppedRecording;

#if UNITY_ANDROID
    private static string s_androidVideoOriginalDir;
#endif

    public static void SetUserId(uint userId)
    {
        s_videoCacheDir = GetVideoCacheDir() + "/" + userId + "/";
#if STANDALONE_RECORDER
        PathConfig.saveFolder = s_videoCacheDir;
#endif
        Init();
    }

    private static void Init()
    {
        if (!s_recorderImpl)
        {
            string recorderName = Application.isMobilePlatform ? "MobileRecorder" : "StandaloneRecorder";
            s_recorderImpl = GameObject.Instantiate(Resources.Load("VideoRecording/" + recorderName)) as GameObject;
#if UNITY_IOS && !UNITY_EDITOR
            s_recorderImpl.name = recorderName;
#endif
            s_recorderImpl.hideFlags = HideFlags.HideAndDontSave;

#if STANDALONE_RECORDER
            VideoCaptureCtrl.instance.eventDelegate.OnComplete += OnRecordingComplete;
            VideoCaptureCtrl.instance.eventDelegate.OnError += OnRecordingError;
            s_recorderImpl.GetComponent<Camera>().enabled = false;
#elif UNITY_ANDROID
            cn.sharerec.ShareRECImpl.SetMinDuration(minimumRecordingSeconds);
            cn.sharerec.ShareREC.OnRecorderStoppedHandler += OnRecordingComplete;

            using (var unity = Utils.GetUnityActivity())
            using (var helper = new AndroidJavaClass("com.mob.tools.utils.ResHelper"))
            {
               s_androidVideoOriginalDir = helper.CallStatic<string>("getCachePath", unity, "videoes");
            }
#else
            com.mob.ShareREC.setMinimumRecordingTime(minimumRecordingSeconds);
            com.mob.ShareREC.setCallbackObjectName(recorderName);
#endif
        }
    }

    private static void OnRecordingComplete()
    {
#if STANDALONE_RECORDER
        lastVideoPath  = VideoCaptureCtrl.instance.lastVideoPaths.FirstOrDefault();
        SetStopped();
#elif UNITY_ANDROID
        var videoPath = Directory.GetFiles(s_androidVideoOriginalDir, "*.mp4")
                                 .OrderByDescending(x => new FileInfo(x).CreationTime)
                                 .FirstOrDefault();
        FinishMobileRecording(videoPath);
#endif
    }

    private static void OnRecordingComplete(Exception e)
    {
        if (e == null)
        {
            // the path is in the /tmp folder, move it to our custom folder
            var path = com.mob.ShareREC.lastRecordingPath();
            var authStatus = GalleryUtils.AuthorizationStatus;
            if (authStatus == GalleryAuthorizationStatus.Authorized ||
                authStatus == GalleryAuthorizationStatus.NotDetermined)
            {
                FinishMobileRecording(path);
            }
            else
            {
                MoveVideoToCache(path);
                SetStopped();
            }
        }
        else
        {
            lastVideoPath = null;
            Debug.LogException(e);
            SetStopped();
        }
    }

    private static void FinishMobileRecording(string path)
    {
        GalleryUtils.SaveVideoToAlbum(path,
            ex => {
                if (ex != null)
                {
                    Debug.LogError("Failed to save video to album");
                }
                MoveVideoToCache(path);
                SetStopped();
            });
    }

    private static void MoveVideoToCache(string path)
    {
        try
        {
            if (!Directory.Exists(s_videoCacheDir))
            {
                Directory.CreateDirectory(s_videoCacheDir);
            }
            var newPath = s_videoCacheDir + "/" + Path.GetFileName(path);
            File.Move(path, newPath);
            lastVideoPath = newPath;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            lastVideoPath = null;
        }
    }

    private static void OnRecordingError(int code)
    {
        Debug.LogError((VideoCaptureCtrlBase.ErrorCodeType)code);
    }

    private static void SetStopped()
    {
        CallbackQueue.instance.RunOnMainThread(() => {
#if STANDALONE_RECORDER
            s_recorderImpl.GetComponent<Camera>().enabled = false;
#endif
            state = State.Stopped;
            if (s_onStoppedRecording != null)
            {
                s_onStoppedRecording();
                s_onStoppedRecording = null;
            }
            if (onStopRecording != null)
            {
                onStopRecording();
            }
        });
    }

    public static bool isAvailable
    {
        get
        {
#if !STANDALONE_RECORDER && UNITY_ANDROID
            return cn.sharerec.ShareREC.IsAvailable();
#else
            return true;
#endif
        }
    }

    public static int minimumRecordingSeconds
    {
        get
        {
            return s_kMinimumRecordingSeconds;
        }
    }

    public static State state
    {
        get;
        private set;
    }

    public static void StartRecording()
    {
        if (state != State.Stopped) { return; }

#if STANDALONE_RECORDER
        // only need to setup the audio capture which is scene specific
        if (!VideoCaptureCtrl.instance.audioCapture)
        {
            var listener = GameObject.FindObjectOfType<AudioListener>();
            if (listener)
            {
                VideoCaptureCtrl.instance.audioCapture = listener.gameObject.AddComponent<AudioCapture>();
            }
            else
            {
                Debug.Log("no AudioListener found, do not record audio");
            }
            s_recorderImpl.GetComponent<Camera>().enabled = true;
        }
        VideoCaptureCtrl.instance.StartCapture();
#elif UNITY_ANDROID
        cn.sharerec.ShareREC.StartRecorder();
#else
        com.mob.ShareREC.startRecording();
#endif

        state = State.Started;
        if (onStartRecording != null)
        {
            onStartRecording();
        }
    }

    public static void StopRecording(Action onStopped = null)
    {
        if (state != State.Started) { return; }

        s_onStoppedRecording = onStopped;
        state = State.Stopping;
#if STANDALONE_RECORDER
        VideoCaptureCtrl.instance.StopCapture();
        s_recorderImpl.GetComponent<Camera>().enabled = false;
#elif UNITY_ANDROID
        cn.sharerec.ShareREC.StopRecorder();
#else
        com.mob.ShareREC.stopRecording(OnRecordingComplete);
#endif
    }

    public static string[] GetVideoPaths()
    {
        if (state != State.Stopped)
        {
            Debug.LogError("not safe to get video paths while recording");
            return new string[0];
        }

        if (Directory.Exists(s_videoCacheDir))
        {
            return Directory.GetFiles(s_videoCacheDir, "*.mp4");
        }
        return new string[0];
    }

    private static string GetVideoCacheDir()
    {
#if STANDALONE_RECORDER
#if UNITY_STANDALONE_WIN
        return PathConfig.myDocumentsPath + "/" + ApplicationUtils.identifier + "/Videos/";
#else
        return PathConfig.myDocumentsPath + "/Documents/" + ApplicationUtils.identifier + "/Videos/";
#endif
#else
        return Application.persistentDataPath + "/videos";
#endif
    }

    /// <summary>
    /// return last recorded video path, null if not found or recording failed
    /// </summary>
    public static string lastVideoPath
    {
        get;
        private set;
    }
}
