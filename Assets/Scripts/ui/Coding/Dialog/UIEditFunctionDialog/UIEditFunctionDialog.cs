using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIEditFunctionDialog : UIInputDialogBase
{
    public UIFunctionPart[] m_partTemplates;
    public RectTransform m_blockTrans;
    public RectTransform m_contentTrans;
    public GameObject m_deleteButton;
    public Button m_okButton;
    public Text m_textError;

    private UIWorkspace m_workspace;
    private FunctionDeclaration m_curDeclaration;

    private readonly List<UIFunctionPart> m_uiParts = new List<UIFunctionPart>();
    private UIFunctionPart m_selectedPart;

    public void Configure(UIWorkspace workspace, FunctionDeclaration declaration = null)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        foreach (var uiPart in m_uiParts)
        {
            Destroy(uiPart.gameObject);
        }
        m_uiParts.Clear();

        _titleText.text = declaration != null ? "ui_edit_function_edit".Localize() : "ui_edit_function_new".Localize();
        m_workspace = workspace;
        m_curDeclaration = declaration;
        m_deleteButton.SetActive(false);

        if (m_curDeclaration != null)
        {
            foreach (var part in m_curDeclaration.parts)
            {
                AddPart(part);
            }
        }
        else
        {
            AddPart(new FunctionPart("ui_edit_function_name_text".Localize(), FunctionPartType.Label));
        }
    }

    private void AddPart(FunctionPart part)
    {
        var uiPart = Instantiate(m_partTemplates[(int)part.type].gameObject).GetComponent<UIFunctionPart>();
        uiPart.gameObject.SetActive(true);
        uiPart.onPointerDown += OnPartPointerDown;
        uiPart.transform.SetParent(m_blockTrans, false);
        uiPart.text = part.text;
        m_uiParts.Add(uiPart);
    }

    public void OnPartPointerDown(UIFunctionPart uiPart)
    {
        // name is mandatory
        if (m_uiParts.IndexOf(uiPart) == 0)
        {
            m_deleteButton.SetActive(false);
            m_selectedPart = null;
            return;
        }

        m_deleteButton.transform.SetParent(uiPart.transform, true);
        var pos = m_deleteButton.transform.localPosition;
        pos.x = 0.0f;
        m_deleteButton.transform.localPosition = pos;
        m_deleteButton.SetActive(true);
        m_selectedPart = uiPart;
    }

    public void OnPartValueChanged(UIFunctionPart uiPart)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_blockTrans);
        var viewportWidth = (m_contentTrans.parent as RectTransform).rect.width;
        m_contentTrans.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, Mathf.Max(m_blockTrans.sizeDelta.x, viewportWidth));

        ValidateFunction();
    }

    private void ValidateFunction()
    {
        string errorText = "";
        // check if the function name is empty
        if (m_uiParts.Count > 0 && m_uiParts[0].text.Trim() == "")
        {
            errorText = "ui_edit_function_block_name_empty".Localize();
        }
        else
        {
            for (int i = 1; i < m_uiParts.Count; ++i)
            {
                var uiPart = m_uiParts[i];
                if (uiPart.type == FunctionPartType.Label)
                {
                    continue;
                }

                if (uiPart.text.Trim() == "")
                {
                    errorText = "ui_edit_function_block_param_name_empty".Localize();
                    break;
                }
                else
                {
                    bool duplicate = m_uiParts.Any(x => x != uiPart &&
                                                        x.type != FunctionPartType.Label &&
                                                        x.text == uiPart.text);
                    if (duplicate)
                    {
                        errorText = "ui_edit_function_block_param_name_duplicate".Localize();
                        break;
                    }
                }
            }
        }

        m_textError.text = errorText;
        m_okButton.interactable = errorText == "";
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIEditFunctionDialog; }
    }

    public void OnClickAddInput(int type)
    {
        var partType = (FunctionPartType)type;

        // do not create two consecutive labels
        if (m_uiParts.Count > 0)
        {
            var lastPart = m_uiParts[m_uiParts.Count - 1];
            if (lastPart.type == FunctionPartType.Label && partType == FunctionPartType.Label)
            {
                lastPart.BeginEdit();
                return;
            }
        }
        
        AddPart(partType);
    }

    private void AddPart(FunctionPartType type)
    {
        if (type == FunctionPartType.Label)
        {
            AddPart(new FunctionPart("ui_edit_function_part_label_text".Localize(), type));
        }
        else
        {
            var paramName = string.Format("ui_edit_function_part_{0}_text", type.ToString().ToLowerInvariant()).Localize();
            paramName += m_uiParts.Count(x => x.type == type) + 1;
            AddPart(new FunctionPart(paramName, type));
        }
    }

    public void OnClickRemovePart()
    {
        if (m_selectedPart)
        {
            m_uiParts.Remove(m_selectedPart);
            m_deleteButton.SetActive(false);
            m_deleteButton.transform.SetParent(m_contentTrans);
            Destroy(m_selectedPart.gameObject);
            m_selectedPart = null;

            ValidateFunction();
        }
    }

    public void OnClickOk()
    {
        var newDecl = new FunctionDeclaration(m_curDeclaration != null ? m_curDeclaration.functionId : Guid.NewGuid());
        foreach (var uiPart in m_uiParts)
        {
            var text = uiPart.text.Trim();
            if (text != "")
            {
                newDecl.AddPart(new FunctionPart(text, uiPart.type));
            }
        }

        if (m_curDeclaration == null)
        {
            m_workspace.UndoManager.AddUndo(new AddFunctionCommand(m_workspace, newDecl));
        }
        else if (!newDecl.Equals(m_curDeclaration))
        {
            m_workspace.UndoManager.AddUndo(new UpdateFunctionCommand(m_workspace, m_curDeclaration, newDecl));
        }

        CloseDialog();
    }
}
