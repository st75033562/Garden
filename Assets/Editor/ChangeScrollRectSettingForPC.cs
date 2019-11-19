using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class ChangeScrollRectSettingForPC
{
    private const float DefaultSensitivity = 40;
    private const ScrollRect.MovementType DefaultMovementType = ScrollRect.MovementType.Clamped;

    [MenuItem("Tools/Change ScrollRect Settings for PC")]
    public static void Do()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        foreach (var sceneSetting in EditorBuildSettings.scenes)
        {
            var scene = EditorSceneManager.OpenScene(sceneSetting.path);
            if (scene.isLoaded)
            {
                foreach (var scrollRect in scene.GetRootGameObjects()
                                                .Select(x => x.GetComponentsInChildren<ScrollRect>(true))
                                                .SelectMany(x => x))
                {
                    // 1 is the default sensitivity
                    if (scrollRect.scrollSensitivity == 1)
                    {
                        scrollRect.scrollSensitivity = DefaultSensitivity;
                    }

                    var scrollSettings = scrollRect.GetComponent<ScrollRectSettings>();
                    /*
                    if (!scrollSettings)
                    {
                        scrollSettings = scrollRect.gameObject.AddComponent<ScrollRectSettings>();
                        scrollSettings.movementType = DefaultMovementType;
                        EditorSceneManager.MarkSceneDirty(scene);
                    }*/
                    if (scrollSettings)
                    {
                        GameObject.DestroyImmediate(scrollSettings);
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }

                if (scene.isDirty && !EditorSceneManager.SaveScene(scene))
                {
                    Debug.LogWarning("Failed to save " + scene.path);
                }
            }
        }

        if (!string.IsNullOrEmpty(activeScene.path))
        {
            EditorSceneManager.OpenScene(activeScene.path);
        }
    }
}
