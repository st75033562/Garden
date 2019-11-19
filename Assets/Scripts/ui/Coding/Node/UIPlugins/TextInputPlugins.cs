using LitJson;
using System;

public class TextInputPlugins : SlotPlugins
{
    private UIEditInputDialogConfig m_config;

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditInputDialog>();
        m_config.content = GetPluginsText();
        dialog.Configure(m_config, this, this);
        OpenDialog(dialog);
    }

    public override void DecodeClickedCMD(string cmd)
    {
        m_config = JsonMapper.ToObject<UIEditInputDialogConfig>(cmd);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_config = ((TextInputPlugins)other).m_config;
    }
}
