using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingIcon;

    void Start()
    {
        SceneDirector.onLoadingScene += OnLoadingScene;
        SceneManager.sceneLoaded += OnSceneLoaded;
        loadingIcon.SetActive(false);
    }

    void OnDestroy()
    {
        SceneDirector.onLoadingScene -= OnLoadingScene;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnLoadingScene(string sceneName)
    {
        loadingIcon.SetActive(sceneName.ToLower() == "main");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadingIcon.SetActive(false);
    }
}
