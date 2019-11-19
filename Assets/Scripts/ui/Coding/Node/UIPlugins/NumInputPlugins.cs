using LitJson;

public class NumInputPlugins : SlotPlugins
{
    private UINumInputDialogConfig m_config;

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UINumInputDialog>();
        m_config.number = GetPluginsText();
        dialog.Configure(m_config, this);
        OpenDialog(dialog);
    }

    public override void DecodeClickedCMD(string cmd)
    {
        m_config = JsonMapper.ToObject<UINumInputDialogConfig>(cmd);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_config = ((NumInputPlugins)other).m_config;
    }
}
