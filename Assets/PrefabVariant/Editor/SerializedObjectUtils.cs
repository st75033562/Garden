using System;
using System.Reflection;
using UnityEditor;

namespace PrefabVariant
{
    public static class SerializedObjectUtils
    {
        public static long GetFileId(UnityEngine.Object obj)
        {
            return GetFileId(new SerializedObject(obj));
        }

        public static long GetFileId(this SerializedObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("property");
            }

            var inspectorModeInfo =
                typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

            inspectorModeInfo.SetValue(obj, InspectorMode.Debug, null);
            var localIdProp = obj.FindProperty("m_LocalIdentfierInFile");
            return localIdProp.longValue;
        }
    }
}
