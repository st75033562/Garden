using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PrefabVariant
{
    /// <summary>
    /// A very simple tool to build prefab variant, also supports nesting prefabs.
    /// Unity can save variant data in scenes, so we simply take advantage of the fact
    /// and update the prefab variant with the instance from the scene.
    ///
    /// IMPORTANT: You should not apply the prefab variant in the editor!!!
    /// </summary>
    [ExecuteInEditMode]
    public class VariantBuilder : MonoBehaviour
    {
        public UnityEngine.Object m_target;

        void Start()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }

#if UNITY_EDITOR
        [ContextMenu("Update Prefab Variant")]
        public void UpdatePrefabVariant()
        {
            if (!m_target)
            {
                string variantPrefabPath;
#if false
                if (PrefabUtility.GetPrefabParent(gameObject))
                {
                    var prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(gameObject));
                    var variantDirPath = Path.GetDirectoryName(prefabPath) + "/Variant";
                    if (!Directory.Exists(variantDirPath))
                    {
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(variantDirPath), Path.GetFileName(variantDirPath));
                    }
                    variantPrefabPath = variantDirPath + "/" + Path.GetFileName(prefabPath);
                }
                else
#endif
                {
                    variantPrefabPath = EditorUtility.SaveFilePanelInProject("Create prefab variant", name, "prefab", "");
                    if (string.IsNullOrEmpty(variantPrefabPath))
                    {
                        return;
                    }
                }

                m_target = PrefabUtility.CreateEmptyPrefab(variantPrefabPath);

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            // update parent prefabs which have us nested
            var rootObjects = SceneManager.GetActiveScene()
                        .GetRootGameObjects()
                        .Except(gameObject)
                        .Select(x => x.GetComponent<VariantBuilder>())
                        .Where(x => x != null)
                        .ToArray();

            // update the target prefab variant
            InternalUpdatePrefab();

            foreach (var root in rootObjects)
            {
                if (HasNested(root.gameObject, m_target))
                {
                    root.InternalUpdatePrefab();
                    Debug.Log("updated referencing prefab: " + root.name, root);
                }
            }

            Debug.Log("updated variant " + name, m_target);
        }

        private void InternalUpdatePrefab()
        {
            if (!m_target)
            {
                Debug.LogErrorFormat(gameObject, "Cannot update {0}, please create the prefab first", name);
                return;
            }

            var instance = Instantiate(gameObject);
            DestroyImmediate(instance.GetComponent<VariantBuilder>());
            m_target = PrefabUtility.ReplacePrefab(instance, m_target, ReplacePrefabOptions.ReplaceNameBased);
            DestroyImmediate(instance);
        }

        private static bool HasNested(GameObject root, UnityEngine.Object nestedPrefab)
        {
            if (PrefabUtility.GetPrefabParent(root) == nestedPrefab)
            {
                return true;
            }

            foreach (Transform child in root.transform)
            {
                if (HasNested(child.gameObject, nestedPrefab))
                {
                    return true;
                }
            }

            return false;
        }
#endif
    }
}
