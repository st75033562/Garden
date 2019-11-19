using PrefabVariant.Editor;
using UnityEditor;
using UnityEngine;

namespace PrefabVariant
{
    public static class ToolCommands
    {
        [MenuItem("Tools/Update Prefab Variant")]
        public static void UpdateVariants()
        {
            foreach (var variant in Selection.GetFiltered<VariantBuilder>(SelectionMode.TopLevel))
            {
                variant.UpdatePrefabVariant();
            }
        }
    }
}
