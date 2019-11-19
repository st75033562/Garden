using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LogicTransform))]
[CanEditMultipleObjects]
public class LogicTransformEditor : Editor
{
    private bool m_childrenExpanded;

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        base.OnInspectorGUI();
        GUI.enabled = true;

        var curTransform = (LogicTransform)this.target;

        EditorGUI.BeginChangeCheck();
        var parent = (LogicTransform)EditorGUILayout.ObjectField("Parent", curTransform.parent, typeof(LogicTransform), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.IncrementCurrentGroup();
            foreach (LogicTransform target in targets)
            {
                Undo.RecordObject(target, "Set Parent");
                if (target.parent != null)
                {
                    Undo.RecordObject(target.parent, "Add Child");
                }
                if (target.lastChild != null)
                {
                    Undo.RecordObject(target.lastChild, "Sibling");
                    if (target.lastChild.prevSibling != null)
                    {
                        Undo.RecordObject(target.lastChild.prevSibling, "Sibling");
                    }
                }
                target.parent = parent;
            }
        }

        if (GUILayout.Button("Print Hierarchy"))
        {
            LogicTransformUtils.PrintHierarchy(curTransform);
        }

        m_childrenExpanded = EditorGUILayout.Foldout(m_childrenExpanded, "Children");
        if (m_childrenExpanded && targets.Length == 1)
        {
            EditorGUI.indentLevel++;
            GUI.enabled = false;
            foreach (var child in curTransform)
            {
                EditorGUILayout.ObjectField(child, child.GetType(), true);
            }
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }
    }
}
