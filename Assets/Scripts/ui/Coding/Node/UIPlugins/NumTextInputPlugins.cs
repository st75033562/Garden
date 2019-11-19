using LitJson;

public class NumTextInputPlugins : SlotPlugins
{
    private UINumTextInputDialogConfig m_config;

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UINumTextInputDialog>();
        m_config.numInputConfig.number = GetPluginsText();
        m_config.textInputConfig.content = GetPluginsText();
        dialog.Configure(m_config, this, this);
        OpenDialog(dialog);
    }

    public override void DecodeClickedCMD(string cmd)
    {
        m_config = JsonMapper.ToObject<UINumTextInputDialogConfig>(cmd);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_config = ((NumTextInputPlugins)other).m_config;
    }
}
