using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IEditView
{
    bool isEditing { get; }
}

public class PopupBaseEditView : PopupController, IEditView
{
    public enum EditMode
    {
        Delete,
        Publish
    }

    public ButtonColorEffect m_backButton;
    public Button m_cancelButton;
    public List<Button> m_groupButtons;
    public GameObject m_emptyGo;

    private Button m_editButton;

    public bool isEditing
    {
        get { return m_editButton != null; }
    }

    protected void AddGroupButtons(params Button[] buttons)
    {
        m_groupButtons.AddRange(buttons);
    }

    protected void BeginEdit(Button button)
    {
        if (button == null)
        {
            throw new ArgumentNullException("modeButton");
        }

        m_editButton = button;
        UpdateUI();
    }

    public virtual void EndEdit()
    {
        m_editButton = null;
        UpdateUI();
    }

    protected virtual void UpdateUI()
    {
        foreach (var button in m_groupButtons)
        {
            button.interactable = button == m_editButton || !isEditing;
        }
        m_backButton.interactable = !isEditing;
        m_cancelButton.gameObject.SetActive(isEditing);
        m_emptyGo.SetActive(isEmpty && !isEditing);
    }

    protected virtual bool isEmpty
    {
        get { return false; }
    }
}
