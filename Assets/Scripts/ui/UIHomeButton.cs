using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIHomeButton : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        PopupManager.CloseAll();
        Utils.GotoHomeScene();
    }
}
