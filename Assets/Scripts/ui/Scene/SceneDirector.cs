using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour
{
    /// <summary>
    /// only called for non popup scenes
    /// </summary>
    public static event Action<string> onLoadingScene;
    public static event Action onLoadingStateChanged;

    private struct SceneRecord
    {
        public SceneController controller;
        public string sceneName;
    }

    private struct SceneHistory
    {
        public string sceneName;
        public object data;
    }

    private static SceneDirector s_instance;
    // scene history, excluding active popups
    private static readonly Stack<SceneHistory> s_history = new Stack<SceneHistory>();
    // active scene + popup scenes
    private static readonly Stack<SceneRecord> s_activeScenes = new Stack<SceneRecord>();
    private static string s_sceneToUnload;
    private static int s_transitioningSceneCount;
    private static bool s_loading;

    private void Awake()
    {
        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public static void Init()
    {
        if (!s_instance)
        {
            var go = new GameObject("SceneDirector");
            go.AddComponent<SceneDirector>();
            var scene = SceneManager.GetActiveScene();
            s_activeScenes.Push(new SceneRecord
            {
                sceneName = scene.name,
                controller = GetSceneController(scene.name)
            });
        }
    }

    public static bool IsLoading
    {
        get { return s_loading; }
        private set
        {
            s_loading = value;
            if (onLoadingStateChanged != null)
            {
                onLoadingStateChanged();
            }
        }
    }

    public static void ClearHistory()
    {
        s_history.Clear();
    }

    /// <summary>
    /// push to the scene, unload the current scene and activate the target scene
    /// </summary>
    /// <param name="sceneName">new scene to load</param>
    /// <param name="userData">data passed to the new scene's controller</param>
    /// <param name="saveData">save data for current scene, if not null, it will be used instead of the one returned by SceneController.OnSaveState</param>
    /// <param name="saveCurSceneOnHistory">true if the current scene need to be pushed on the history stack</param>
    /// <returns></returns>
    public static bool Push(string sceneName, object userData = null, object saveData = null, bool saveCurSceneOnHistory = true)
    {
        return Push(sceneName, userData, false, saveData, true, saveCurSceneOnHistory);
    }

    private static bool Push(string sceneName, object userData, bool isRestore, object saveData,
                             bool transitionOutCurScene, bool saveCurSceneOnHistory)
    {
        if (IsLoading)
        {
            Debug.LogWarning("already loading, ignore new loading request");
            return false;
        }

        // Debug.Log("load scene: " + sceneName);
        s_instance.StartCoroutine(LoadLevel(sceneName, userData, isRestore, saveData, 
                                            transitionOutCurScene, saveCurSceneOnHistory));
        return true;
    }

    // transitionOutCurScene: false for pop up scenes
    private static IEnumerator LoadLevel(string sceneName, object userData, bool isRestore, object saveData,
                                         bool transitionOutCurScene, bool saveCurSceneOnHistory)
    {
        AsyncOperation asyncLoadOp;
        asyncLoadOp = SceneManager.LoadSceneAsync(sceneName, transitionOutCurScene ? LoadSceneMode.Single : LoadSceneMode.Additive);

        var nextScene = SceneManager.GetSceneByName(sceneName);
        if (!nextScene.IsValid())
        {
            Debug.LogError("invalid scene: " + sceneName);
            yield break;
        }

        IsLoading = true;

        if (onLoadingScene != null && transitionOutCurScene)
        {
            onLoadingScene(sceneName);
        }

        // disable event system until loading completes
        EventSystem.current.enabled = false;

        var curSceneRecord = transitionOutCurScene ? s_activeScenes.Pop() : new SceneRecord();
        s_sceneToUnload = null;

        if (transitionOutCurScene)
        {
            // do not save if reloading current scene
            if (saveCurSceneOnHistory && curSceneRecord.sceneName != sceneName)
            {
                if (saveData == null)
                {
                    saveData = curSceneRecord.controller != null
                                   ? curSceneRecord.controller.OnSaveState()
                                   : null;
                }
                s_history.Push(new SceneHistory {
                    sceneName = curSceneRecord.sceneName,
                    data = saveData
                });
            }
        }

        yield return asyncLoadOp;

        SceneController nextSceneController = GetSceneController(sceneName);
        s_activeScenes.Push(new SceneRecord {
            sceneName = sceneName,
            controller = nextSceneController
        });

        if (nextSceneController)
        {
            ++s_transitioningSceneCount;
            nextSceneController.Init(userData, isRestore);
            nextSceneController.BeginTransition(SceneTransition.Direction.In);
        }
        else
        {
            s_instance.StartCoroutine(FinishLoading());
        }
    }

    private static IEnumerator FinishLoading()
    {
        if (s_sceneToUnload != null)
        {
            yield return SceneManager.UnloadSceneAsync(s_sceneToUnload);
            s_sceneToUnload = null;
        }

        // after unloading, event system might have been destroyed,
        // try to enable a new event system in the top level scene
        if (!EventSystem.current)
        {
            var topSceneName = s_activeScenes.Peek().sceneName;
            var scene = SceneManager.GetSceneByName(topSceneName);
            foreach (var root in scene.GetRootGameObjects())
            {
                var eventSytem = root.GetComponent<EventSystem>();
                if (eventSytem)
                {
                    eventSytem.enabled = true;
                    break;
                }
            }

            if (!EventSystem.current)
            {
                Debug.LogError("No event system found");
                // TODO: create a new event system ?
            }
        }
        else
        {
            EventSystem.current.enabled = true;
        }

        IsLoading = false;
    }

    private static SceneController GetSceneController(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponent<SceneController>();
                if (controller)
                {
                    return controller;
                }
            }
        }
        return null;
    }

    internal static void OnSceneTransitionEnd(SceneController controller)
    {
        if (--s_transitioningSceneCount == 0)
        {
            s_instance.StartCoroutine(FinishLoading());
        }
    }

    // show the popup, current scene is not changed
    public static bool PushPopup(string sceneName, object userData = null)
    {
        return Push(sceneName, userData, false, null, false, false);
    }

    // if there's any pop up, it will be unloaded
    // if there's any scene history, go to the last active scene
    public static bool Pop()
    {
        if (IsLoading)
        {
            return false;
        }

        // check if there's any popup scenes
        if (s_activeScenes.Count > 1)
        {
            var record = s_activeScenes.Pop();
            if (record.controller)
            {
                record.controller.BeginTransition(SceneTransition.Direction.Out);
                ++s_transitioningSceneCount;
            }

            s_sceneToUnload = record.sceneName;

            return true;
        }

        if (s_history.Count > 0)
        {
            var history = s_history.Pop();
            return Push(history.sceneName, history.data, true, null, true, false);
        }
        else
        {
            // TODO: better to move to app level
            if (Application.isMobilePlatform)
            {
                Application.Quit();
            }
            return false;
        }
    }

    public static string CurrentSceneName()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.name;
    }
}
