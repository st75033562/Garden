using UnityEngine.UI;

//public enum LEDColorEnum
//{
//	LED_OFF,
//	LED_BLUE,
//	LED_GREEN,
//	LED_CYAN,
//	LED_RED,
//	LED_MAGENTA,
//	LED_YELLOW,
//	LED_WHITE,
//}

public class UIColorSelectDialogConfig
{
    public string title;
}

public class UIColorSelectDialog : UIInputDialogBase
{
    public ColorSelectionSettings m_ColorSettings;
    public Text m_TitleText;
    public Image[] m_Images;

    private IDialogInputCallback m_Callback;

    public override void Init()
    {
        base.Init();

        for (int i = 0; i < m_Images.Length; ++i)
        {
            m_Images[i].color = m_ColorSettings.colors[i];
        }
    }

    public void Configure(UIColorSelectDialogConfig config, NodePluginsBase plugin)
    {
        m_TitleText.text = config.title.Localize();
        m_Callback = plugin;
    }

	public void SetLedColor(int colorID)
	{
		if (m_Callback != null)
		{
			m_Callback.InputCallBack(colorID.ToString());
		}
		CloseDialog();
	}

    public override UIDialog dialogType
    {
        get { return UIDialog.UIColorSelectDialog; }
    }
}
