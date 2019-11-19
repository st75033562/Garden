using System;
using UnityEngine.UI;

public class UIPopupYesNo : PopupController
{
    public Text m_acceptText;
    public Text m_rejectText;
    public Image m_mask;

    public Action accetAction { get; set; }
    public Action rejectAction { get; set; }

    public void OnClickAccept()
    {
        Close();
        if (accetAction != null)
        {
            accetAction();
        }
    }

    public void OnClickReject()
    {
        Close();
        if (rejectAction != null)
        {
            rejectAction();
        }
    }

    public void SetAcceptText(string text)
    {
        m_acceptText.text = !string.IsNullOrEmpty(text) ? text : "ui_confirm".Localize();
    }

    public void SetRejectText(string text)
    {
        m_rejectText.text = !string.IsNullOrEmpty(text) ? text : "ui_cancel".Localize();
    }

    public bool modal
    {
        get { return m_mask.enabled; }
        set { m_mask.enabled = value; }
    }
}
