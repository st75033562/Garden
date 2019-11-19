using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Robomation;
using System.Collections.Generic;
using LitJson;

//	LED_OFF = 0,
//	LED_BLUE,
//	LED_GREEN,
//	LED_CYAN,
//	LED_RED,
//	LED_MAGENTA,
//	LED_YELLOW,
//	LED_WHITE,

public class SelectColorPlugins : NodePluginsBase
{
    public ColorSelectionSettings m_ColorSettings;
	public Image m_Select;

    [SerializeField]
    private int m_ColorID = Hamster.LED_RED;

    private UIColorSelectDialogConfig m_config;

	protected override void Start()
	{
		UpdateUI();
	}

	protected override void OnInput(string str)
	{
		base.OnInput(str);
		int colorId = 0;

		if (int.TryParse(str, out colorId))
		{
            this.colorId = colorId;
            MarkChanged();
		}
	}

	public void UpdateUI()
	{
        m_Select.color = color;
	}

	public int colorId
	{
        get { return m_ColorID; }
        set
        {
            m_ColorID = value;
            UpdateUI();
        }
	}

    public Color color
    {
        get { return m_ColorSettings.colors[m_ColorID]; }
    }

	public override Save_PluginsData GetPluginSaveData()
	{
		Save_PluginsData tSave = base.GetPluginSaveData();
		tSave.PluginIntValue = (int)m_ColorID;
		return tSave;
	}

	public override void LoadPluginSaveData(Save_PluginsData save)
	{
		base.LoadPluginSaveData(save);
		m_ColorID = save.PluginIntValue;
        UpdateUI();
	}

    public override void DecodeClickedCMD(string cmd)
    {
        m_config = JsonMapper.ToObject<UIColorSelectDialogConfig>(cmd);
    }

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIColorSelectDialog>();
        dialog.Configure(m_config, this);
        OpenDialog(dialog);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_config = ((SelectColorPlugins)other).m_config;
    }
}
