using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodePanelToolbar : MonoBehaviour
{
    public GameObject m_CopyPasteMask;
    public Toggle m_CopyBtn;
    public Button m_SystemBtn;
    public UnityCommandManager m_CommandManager;

	public GameObject m_ShowMessageBtn;
	public GameObject m_HideMessageBtn;
    public LeaveMessagePanel m_MessagePanel;
    public UIWorkspace m_Workspace;

    private bool m_messageButtonVisible = true;
    private bool m_copyEnabled = true;

    void Awake()
    {
        m_MessagePanel.OnActivated.AddListener(OnMessagePanelActivated);
        m_CommandManager.enabled = false;
    }

    public bool CopyEnabled
    {
        get { return m_copyEnabled; }
        set
        {
            m_copyEnabled = value;
            UpdateCopyButton();
        }
    }

    public bool SystemButtonEnabled
    {
        get { return m_SystemBtn.interactable; }
        set { m_SystemBtn.interactable = value; }
    }

    public bool MessageButtonVisible
    {
        get { return m_messageButtonVisible; }
        set
        {
            m_messageButtonVisible = value;
            UpdateMessageButton();
        }
    }

    public void OnClickCopyButton()
    {
        m_CopyPasteMask.SetActive(true);
        m_CommandManager.enabled = true;
    }

    public void OnClickPaste(BaseEventData eventData)
    {
        CancelCopy();

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll((PointerEventData)eventData, raycastResults);

        // check any non template node is clicked
        foreach (var result in raycastResults)
        {
            var node = result.gameObject.GetComponentInParent<FunctionNode>();
            if (node && !node.IsTemplate)
            {
                const float Offset = 20;
                WorkspaceUtils.CopyNodesToClipboard(m_Workspace, node);
                WorkspaceUtils.PasteNodesFromClipboard(m_Workspace,
                    node.RectTransform.position + new Vector3(Offset, -Offset));
                break;
            }
        }
    }

    public bool IsCopying
    {
        get { return m_CopyPasteMask.activeSelf; }
    }

    public void CancelCopy()
    {
        m_CopyPasteMask.SetActive(false);
        m_CopyBtn.isOn = false;
        m_CommandManager.enabled = false;
    }

    void OnMessagePanelActivated(bool active)
    {
        UpdateCopyButton();
        UpdateMessageButton();
    }

    private void UpdateCopyButton()
    {
        m_CopyBtn.interactable = CopyEnabled && !m_MessagePanel.IsActive;
    }

    private void UpdateMessageButton()
    {
        m_ShowMessageBtn.SetActive(!m_MessagePanel.IsActive && m_messageButtonVisible);
        m_HideMessageBtn.SetActive(m_MessagePanel.IsActive && m_messageButtonVisible);
    }
}
