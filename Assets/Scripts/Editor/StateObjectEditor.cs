using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

[CustomEditor(typeof(StateObject))]
public class StateObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var obj = target as StateObject;
        bool on = EditorGUILayout.Toggle("state", obj.on);
        obj.on = on;
    }
}
