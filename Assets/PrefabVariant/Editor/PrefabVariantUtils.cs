using System;
using UnityEditor;
using UnityEngine;

namespace PrefabVariant.Editor
{
    public static class PrefabVariantUtils
    {
        public static void DeriveFrom(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            if (prefab.GetComponent<ReferenceCollection>())
            {
                Debug.LogError("The prefab has already inherited another prefab");
                return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);
            var newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(path);
            var newPrefab = PrefabUtility.CreatePrefab(newPrefabPath, prefab);

            PrefabUpdator.LinkToParent(newPrefab.transform, prefab.transform);
            foreach (var refs in newPrefab.GetComponentsInChildren<ReferenceCollection>(true))
            {
                PrefabUpdator.InitReferences(refs);
            }

            EditorUtility.SetDirty(newPrefab);
        }

        public static void BreakInheritance(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("prefab");
            }

            foreach (var comp in prefab.GetComponentsInChildren<IPrefabComponent>(true))
            {
                Undo.DestroyObjectImmediate((MonoBehaviour)comp);
            }
        }
    }
}
