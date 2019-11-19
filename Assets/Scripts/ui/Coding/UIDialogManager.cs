using System;
using System.Collections.Generic;

public enum UIDialog
{
	UINumInputDialog,
	UINumSelectDialog,
	UIBoolSelectDialog,
	UIMenuDialog,
	UIEditInputDialog,
	UIColorSelectDialog,
	UINoticeDialog,
	UIOperationDialog,
	UISystemSettingsDialog,
	UIAddDataDialog,
	UIDeleteDataDialog,
	UIMonitorDialog,
	UIDeleteMsgDialog,
    UINumTextInputDialog,
    UIEditMessageDialog,
    UIEditDataDialog,
    UIGrayColorDialog,
    UIARMarkerDialog,
    UIARObjectDialog,
    UIEditFunctionDialog,
}

public delegate void DialogInputHandler(string input);
public delegate string DialogInputValidationHandler(string input);

public interface IDialogInputCallback
{
    void InputCallBack(string value);
}

public class DialogInputCallback : IDialogInputCallback
{
    private readonly DialogInputHandler m_callback;

    public DialogInputCallback(DialogInputHandler callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException();
        }

        m_callback = callback;
    }

    public void InputCallBack(string value)
    {
        m_callback(value);
    }
}

public interface IDialogInputValidator
{
    string ValidateInput(string value);
}

public class DialogInputValidator : IDialogInputValidator
{
    private readonly DialogInputValidationHandler m_callback;

    public DialogInputValidator(DialogInputValidationHandler callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException();
        }

        m_callback = callback;
    }

    public string ValidateInput(string value)
    {
        return m_callback(value);
    }
}

public class UIDialogManager
{
    private Dictionary<UIDialog, string> m_DialogNames;

    public static readonly UIDialogManager g_Instance = new UIDialogManager();

    private UIDialogManager()
    {
        m_DialogNames = new Dictionary<UIDialog, string>();
        foreach (UIDialog dialog in Enum.GetValues(typeof(UIDialog)))
        {
            m_DialogNames.Add(dialog, "dialogs/" + dialog.ToString().Substring(2));
        }
    }

    public T GetDialog<T>() where T : UIInputDialogBase
    {
        return (T)GetDialog((UIDialog)Enum.Parse(typeof(UIDialog), typeof(T).Name));
    }

    public UIInputDialogBase GetDialog(UIDialog type)
    {
        var dialog = (UIInputDialogBase)PopupManager.Create(m_DialogNames[type]);
        dialog.Init();
        return dialog;
    }
}
