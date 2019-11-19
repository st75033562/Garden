using UnityEngine;
using UnityEngine.UI;

public class UINumSelectDialogConfig
{
    public string title;
    public int count;
}

public class UINumSelectDialog : UIInputDialogBase
{
	public Text m_Title;
    public RectTransform m_Container;
	public GameObject m_Template;

    private IDialogInputCallback m_Callback;

    private void RefreshCountList(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            GameObject instance = Instantiate(m_Template, m_Container) as GameObject;
            instance.SetActive(true);
            instance.name = i.ToString();

            UIBotIndex botIndex = instance.GetComponent<UIBotIndex>();
            botIndex.m_Index.text = i.ToString();
        }
    }

	public void SelectNum(Text obj)
	{
		if (m_Callback != null)
		{
			m_Callback.InputCallBack(obj.text);
		}
		CloseDialog();
    }

	public void Configure(UINumSelectDialogConfig config, IDialogInputCallback callback)
	{
        m_Callback = callback;
        m_Title.text = config.title.Localize();

        RefreshCountList(Mathf.Max(config.count, 1));
	}

    public override UIDialog dialogType
    {
        get { return UIDialog.UINumSelectDialog; }
    }
}
