using AOT;
using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

public enum GalleryAuthorizationStatus
{
    NotDetermined,
    Restricted,
    Denied,
    Authorized
}

public enum GalleryPickMediaType
{
    Image,
    Video = 2
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void GalleryAuthroizationCallback(GalleryAuthorizationStatus success);

public class GalleryUtils
{
#if !UNITY_EDITOR && UNITY_IOS

    [DllImport("__Internal")]
    private static extern int gu_video_compatible_with_album(string path);

    [DllImport("__Internal")]
    private static extern GalleryAuthorizationStatus gu_album_authorization_status();

    [DllImport("__Internal")]
    private static extern void gu_album_request_authorization(GalleryAuthroizationCallback cb);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SaveVideoCallback(string error, IntPtr userInfo);

    [DllImport("__Internal")]
    private static extern void gu_save_video_at_path_to_album(string path, SaveVideoCallback cb, IntPtr userInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void PickMediaCallback(string path);

    [DllImport("__Internal")]
    private static extern void gu_pick_media(GalleryPickMediaType type, PickMediaCallback cb);

    [DllImport("__Internal")]
    private static extern void gu_remove_temp_files();

    private class SaveVideoContext
    {
        public Action onComplete;
        public Action<Exception> onError;
        public string videoPath;
    }

    private static SaveVideoCallback s_videoCallback;
    private static PickMediaCallback s_pickMediaCallback;
    private static Action<string>    s_pickMediaUserCallback;
    private static GalleryAuthroizationCallback s_authorizationCallback;

    static GalleryUtils()
    {
        s_videoCallback = OnSaveVideoToAlbumComplete;
        s_pickMediaCallback = OnPickMedia;
    }

#endif

    public static GalleryAuthorizationStatus AuthorizationStatus
    {
#if UNITY_IOS && !UNITY_EDITOR
        get { return gu_album_authorization_status(); }
#else
        get { return GalleryAuthorizationStatus.Authorized; }
#endif
    }

    public static void Authorize(GalleryAuthroizationCallback cb)
    {
        if (cb == null)
        {
            throw new ArgumentNullException("cb");
        }
#if !UNITY_EDITOR && UNITY_IOS
        s_authorizationCallback = cb;
        gu_album_request_authorization(OnAuthorized);
#else
        cb(GalleryAuthorizationStatus.Authorized);
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(GalleryAuthroizationCallback))]
    private static void OnAuthorized(GalleryAuthorizationStatus status)
    {
        var cb = s_authorizationCallback;
        s_authorizationCallback = null;
        try
        {
            cb(status);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
#endif

    public static void SaveVideoToAlbum(string path, Action<Exception> onComplete)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            using (var env = new AndroidJavaClass("android.os.Environment"))
            {
                var DIRECTORY_DCIM = env.GetStatic<string>("DIRECTORY_DCIM");
                var pathFile = env.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", DIRECTORY_DCIM);
                var cameraFolderPath = Path.Combine(pathFile.Call<string>("getAbsolutePath"), "Camera");
                if (!Directory.Exists(cameraFolderPath))
                {
                    Directory.CreateDirectory(cameraFolderPath);
                }

                new Thread(() => {
                    try
                    {
                        File.Copy(path, Path.Combine(cameraFolderPath, Path.GetFileName(path)));
                        if (onComplete != null)
                        {
                            CallbackQueue.instance.Enqueue(() => { onComplete(null); });
                        }
                    }
                    catch (Exception e)
                    {
                        if (onComplete != null)
                        {
                            CallbackQueue.instance.Enqueue(() => { onComplete(e); });
                        }
                    }
                }).Start();
            }
        }
        catch (Exception e)
        {
            if (onComplete != null)
            {
                onComplete(e);
            }
        }
#elif !UNITY_EDITOR && UNITY_IOS
        var context = new SaveVideoContext {
            onComplete = () => onComplete(null),
            onError = onComplete,
            videoPath = path
        };
        var ctxHandle = GCHandle.Alloc(context);
        gu_save_video_at_path_to_album(path, s_videoCallback, GCHandle.ToIntPtr(ctxHandle));
#endif
    }

#if !UNITY_EDITOR && UNITY_IOS
    [MonoPInvokeCallback(typeof(SaveVideoCallback))]
    private static void OnSaveVideoToAlbumComplete(string error, IntPtr userInfo)
    {
        var ctxHandle = GCHandle.FromIntPtr(userInfo);
        var context = (SaveVideoContext)ctxHandle.Target;

        try
        {
            if (error != null)
            {
                if (context.onError != null)
                {
                    context.onError(new ApplicationException(error));
                }
                else
                {
                    Debug.LogErrorFormat("failed to copy {0}, error {1}", context.videoPath, error);
                }
            }
            else
            {
                if (context.onComplete != null)
                {
                    context.onComplete();
                }
                else
                {
                    Debug.LogFormat("copied {0}", context.videoPath);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            ctxHandle.Free();
        }
    }
#endif

    public static void Pick(GalleryPickMediaType type, Action<string> cb)
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (s_pickMediaUserCallback != null)
        {
            throw new InvalidOperationException("operation in progress");
        }
        s_pickMediaUserCallback = cb;
        gu_pick_media(type, OnPickMedia);
#else
        Debug.LogError("not implemented");
#endif
    }

    public static void RemoveTempFiles()
    {
#if UNITY_IOS && !UNITY_EDITOR
        gu_remove_temp_files();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(PickMediaCallback))]
    private static void OnPickMedia(string path)
    {
        try
        {
            if (s_pickMediaUserCallback != null)
            {
                s_pickMediaUserCallback(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            s_pickMediaUserCallback = null;
        }
    }
#endif
}
