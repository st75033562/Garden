using UnityEngine;

public class PopupTwoBtnDialog : PopupController
{
    public GameObject m_closeButton;

    public void ShowCloseButton(bool visible)
    {
        m_closeButton.SetActive(visible);
    }
}
