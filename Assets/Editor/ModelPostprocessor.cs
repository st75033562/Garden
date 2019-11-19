using UnityEditor;
using UnityEngine;

public class ModelPostprocessor : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject go)
    {
        var importer = (ModelImporter)assetImporter;
        if (importer.assetPath.StartsWith("Assets/Simulation/"))
        {
            if (importer.isReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }
}
