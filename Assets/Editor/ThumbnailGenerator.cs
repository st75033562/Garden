using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThumbnailGenerator
{
    private const string MaskPath = "Assets/Simulation/Misc/thumbnail_mask.png";

    [MenuItem("Assets/Create Gameboard Thumbnail")]
    public static void Generate()
    {
        var texture = Selection.activeObject as Texture2D;
        if (texture != null)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            EditorUtils.Run("python", "Tools/mask.py", path, MaskPath);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 2048;
                importer.anisoLevel = 0;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
    }

    [MenuItem("Assets/Create Gameboard Thumbnail", true)]
    public static bool IsTexture()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("Tools/Save Gameboard Thumbnail")]
    public static void Snapshot()
    {
        string initialDir = Application.dataPath;
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.name.Equals("gameboardscene", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(scene.path))
                {
                    initialDir = Path.GetDirectoryName(scene.path);
                    break;
                }
            }
        }

        string parentDir = EditorUtility.SaveFolderPanel("Select the parent folder to save thumbnail", initialDir, "thumbnail");
        if (parentDir.Length != 0)
        {
            parentDir = EditorUtils.GetProjectRelativePath(parentDir);
            if (!Directory.Exists(parentDir))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(parentDir), Path.GetFileName(parentDir));
            }
            EditorUtils.CaptureScreenshot(parentDir + "/thumbnail.png");
        }
    }
}
