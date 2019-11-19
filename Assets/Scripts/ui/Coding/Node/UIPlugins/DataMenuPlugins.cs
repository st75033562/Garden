using System.Linq;
using UnityEngine;

public class DataMenuPlugins : DownMenuPlugins
{
    private static readonly Color GlobalDataColor = Color.red;
    private const string MenuItemVariableNotSet = "data_menu_variable_not_set";
    private const string MenuItemSelectVariable = "variable_menu_click_to_choose";

    protected BlockVarType m_Type;

    private bool m_VariableSelected;

    protected override void Awake()
    {
        base.Awake();
    }

    private void InitListeners()
    {
        if (CodeContext != null)
        {
            CodeContext.variableManager.onVariableAdded.AddListener(OnVariableAdded);
            CodeContext.variableManager.onVariableRemoved.AddListener(OnVariableRemoved);
            CodeContext.variableManager.onVariablesCleared.AddListener(ResetSelection);
            CodeContext.variableManager.onVariableRenamed.AddListener(OnVariableRenamed);
        }
    }

    protected void OnDestroy()
    {
        if (CodeContext != null)
        {
            CodeContext.variableManager.onVariableAdded.RemoveListener(OnVariableAdded);
            CodeContext.variableManager.onVariableRemoved.RemoveListener(OnVariableRemoved);
            CodeContext.variableManager.onVariablesCleared.RemoveListener(ResetSelection);
            CodeContext.variableManager.onVariableRenamed.RemoveListener(OnVariableRenamed);
        }
    }

    // true if the data menu allows not setting variables
    public bool allowNotSet
    {
        get;
        set;
    }

    private void OnVariableAdded(BaseVariable data)
    {
        if (data.type == m_Type && m_MyNode.IsTemplate && !allowNotSet)
        {
            SelectVariable();
        }
    }

    private void OnVariableRemoved(BaseVariable data)
    {
        if (data.type == m_Type && data.name == m_TextKey)
        {
            ResetSelection();
        }
    }

    public override void ResetSelection()
    {
        bool wasSelected = m_VariableSelected;
        m_VariableSelected = false;
        if (m_MyNode.IsTemplate && !allowNotSet)
        {
            SelectVariable();
        }
        if (!m_VariableSelected && wasSelected)
        {
            if (allowNotSet)
            {
                SetPluginsText(MenuItemVariableNotSet);
            }
            else
            {
                SetPluginsText(MenuItemSelectVariable);
            }
            MarkChanged();
        }
    }

    private void OnVariableRenamed(BaseVariable data, string oldName)
    {
        if (m_TextKey == oldName)
        {
            SetPluginsText(data.name);
            LayoutChanged();
        }
    }

    protected override void OnInput(string str)
    {
        base.OnInput(str);

        UpdateSelected();
    }

    private void UpdateSelected()
    {
        m_VariableSelected = CodeContext.variableManager.get(m_TextKey) != null;
    }

    public override void Clicked()
    {
        var menuItems = CodeContext.variableManager.allVarsOfType(m_Type)
                            .OrderByDescending(x => x.scope)
                            .ThenBy(x => x.name)
                            .Select(x => new UIMenuItem(x.name, GetDataColor(x)));
        if (allowNotSet)
        {
            menuItems = new[] { new UIMenuItem(MenuItemVariableNotSet) }.Concat(menuItems);
        }
        SetMenuItems(menuItems);
        base.Clicked();
    }

    private static Color GetDataColor(BaseVariable x)
    {
        return x != null && x.scope == NameScope.Global ? GlobalDataColor : UIMenuItem.DefaultColor;
    }

    private void SelectVariable()
    {
        if (!m_VariableSelected)
        {
            foreach (var item in CodeContext.variableManager.allVarsOfType(m_Type))
            {
                SetPluginsText(item.name);
                LayoutChanged();
                break;
            }
        }
    }

    public override void SetPluginsText(string str)
    {
        base.SetPluginsText(str);

        if (CodeContext != null)
        {
            UpdateSelected();
            var variable = m_VariableSelected ? CodeContext.variableManager.get(str) : null;
            m_Text.color = GetDataColor(variable);
        }
        else
        {
            m_VariableSelected = false;
        }
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        base.LoadPluginSaveData(save);

        var isValid = CodeContext.variableManager.get(m_TextKey) != null;
        if (!isValid)
        {
            m_VariableSelected = true;
            ResetSelection();
        }
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        var rhs = other as DataMenuPlugins;
        m_Type = rhs.m_Type;
        m_VariableSelected = rhs.m_VariableSelected;
        allowNotSet = rhs.allowNotSet;

        InitListeners();
    }
}
