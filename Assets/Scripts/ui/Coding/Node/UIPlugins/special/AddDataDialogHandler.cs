using System;

public class AddDataDialogHandler : IDialogInputCallback, IDialogInputValidator
{
    private readonly VariableManager m_varManager;
    private readonly Action<UIEditDataDialogResult> m_onAddVariable;

    public AddDataDialogHandler(VariableManager varManager, Action<UIEditDataDialogResult> onAddVaraible)
    {
        m_varManager = varManager;
        m_onAddVariable = onAddVaraible;
    }

    public void InputCallBack(string value)
    {
        if (m_onAddVariable != null)
        {
            m_onAddVariable(UIEditDataDialogResult.Parse(value));
        }
    }

	public string ValidateInput(string value)
	{
		if (m_varManager.get(value) != null)
		{
            return "name_already_in_use".Localize();
		}

        if (VariableManager.isReserved(value))
        {
            return "ui_error_name_reserved".Localize();
        }

        return null;
	}
}
