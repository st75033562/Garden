using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class InputMenu
{
    [MenuItem("Tools/Input/Add Escape Command")]
    public static void AddEscapeCommand()
    {
        foreach (var go in Selection.GetFiltered<GameObject>(SelectionMode.TopLevel))
        {
            var cmdManager = go.GetComponent<UnityCommandManager>();
            if (!cmdManager)
            {
                cmdManager = go.AddComponent<UnityCommandManager>();
            }

            var cmd = new UnityKeyCommand();
            cmd.key = KeyCode.Escape;
            cmd.targetButton = go.GetComponent<Button>();
            if (!cmdManager.Contains(cmd))
            {
                cmdManager.Add(cmd);
            }
        }
    }
}
