using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIWorkspace))]
public class UIWorkspaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var workspace = (UIWorkspace)target;

        if (workspace.UndoManager != null)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = workspace.UndoManager.UndoStackSize > 0;
            if (GUILayout.Button("Undo"))
            {
                workspace.Undo();
            }

            GUI.enabled = workspace.UndoManager.RedoStackSize > 0;
            if (GUILayout.Button("Redo"))
            {
                workspace.Redo();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
