public class UIBoolSelectDialog : UIInputDialogBase
{
    private IDialogInputCallback m_Callback;

	public void SelectBool(string select)
	{
		if (m_Callback != null)
		{
			m_Callback.InputCallBack(select);
		}
		CloseDialog();
	}

    public void Configure(NodePluginsBase plugin)
    {
        m_Callback = plugin;
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIBoolSelectDialog; }
    }
}
