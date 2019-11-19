using System.Collections.Generic;
using System.Linq;

public class DownMenuPlugins : NodePluginsBase
{
    private readonly UIMenuConfig m_config = new UIMenuConfig();

    protected override void Start()
    {
        m_config.target = m_Rect;
        UpdateText();
    }

    public override void DecodeClickedCMD(string cmd)
    {
        m_config.FromJson(cmd);
    }

	protected virtual void UpdateText()
	{
		SetPluginsText(m_TextKey);
	}

    public virtual void ResetSelection()
    {
        if (m_config.items.Length > 0)
        {
            SetPluginsText(m_config.items[0].text);
        }
    }

	public virtual string GetMenuValue()
	{
		return m_TextKey;
	}

	protected override void OnInput(string str)
	{
        EventBus.Default.AddEvent(EventId.GuideInput, new GuideInputBackData(gameObject, str));

        base.OnInput(str);
		ChangePluginsText(str);
	}

	public override Save_PluginsData GetPluginSaveData()
	{
		Save_PluginsData tSave = base.GetPluginSaveData();
		tSave.PluginTextValue = m_TextKey;
		return tSave;
	}

	public override void LoadPluginSaveData(Save_PluginsData save)
	{
		base.LoadPluginSaveData(save);
		m_TextKey = save.PluginTextValue;
		UpdateText();
	}

    protected void SetMenuItems<T>(IEnumerable<T> items)
    {
        m_config.items = items.Select(x => new UIMenuItem(x.ToString())).ToArray();
    }

    protected void SetMenuItems(IEnumerable<UIMenuItem> items)
    {
        m_config.items = items.ToArray();
    }

    public override void Clicked()
    {
        if (m_config.items.Length > 0)
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UIMenuDialog>();
            dialog.Configure(m_config, InputCallBack, CodeContext.panelZoomFactor);
            OpenDialog(dialog);
        }
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        var rhs = other as DownMenuPlugins;
        m_config.items = rhs.m_config.items;
        m_config.bgColor = rhs.m_config.bgColor;
    }
}
