using LitJson;
using System;
using System.Collections;
using UnityEngine;

public class LinkSlotPluginsConfig
{
    public string numInputConfig;
    public string menuConfig;
}

public class LinkSlotPlugins : SlotPlugins
{
    public LinkSlotSubSlotPlugins m_Slot;
    public LinkSlotSubDownMenuPlugins m_Menu;

    protected override void OnInput(string str)
    {
        base.OnInput(str);
        m_Slot.SetPluginsText(str);
    }

    public override FunctionNode ParentNode
    {
        set
        {
            base.ParentNode = value;
            m_Slot.ParentNode = value;
            m_Menu.ParentNode = value;
        }
    }

    public override void SetPluginsText(string str)
    {
        m_Slot.SetPluginsText(str);
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            m_OriginalSize.x = m_Slot.RectTransform.rect.width + m_Menu.RectTransform.rect.width;
        }
    }
    public override string GetPluginsText()
    {
        return m_Slot.GetPluginsText();
    }

    public override IEnumerator GetSlotValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        if (m_Insert)
        {
            yield return m_Insert.GetReturnValue(context, retValue);
        }
        else
        {
            retValue.value = m_Slot.GetPluginsText();
        }
    }

    public override void DecodeClickedCMD(string cmd)
    {
        var config = JsonMapper.ToObject<LinkSlotPluginsConfig>(cmd);
        m_Menu.DecodeClickedCMD(config.menuConfig);
        m_Slot.DecodeClickedCMD(config.numInputConfig);
    }

    protected override void ShowBackground(bool visible)
    {
        base.ShowBackground(visible);

        m_Menu.gameObject.SetActive(visible);
        m_Slot.gameObject.SetActive(visible);
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        Save_PluginsData tSave = base.GetPluginSaveData();
        tSave.PluginTextValue = m_Slot.GetPluginsText();
        return tSave;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        base.LoadPluginSaveData(save);
        m_Slot.SetPluginsText(save.PluginTextValue);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        var rhs = (LinkSlotPlugins)other;
        m_Menu.PostClone(rhs.m_Menu);
        m_Slot.PostClone(rhs.m_Slot);
    }
}
