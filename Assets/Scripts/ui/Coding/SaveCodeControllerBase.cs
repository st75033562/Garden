using System;

public enum SaveCodeStatus
{
    Saved,     // content was saved successfully
    Discard,   // content was discarded
    Unchanged, // content was not changed
}

public abstract class SaveCodeControllerBase
{
    public void SaveWithConfirm(Action<SaveCodeStatus> onDone, Action onCancel = null)
    {
        Action<SaveCodeStatus> callback = (res) => {
            if (onDone != null)
            {
                onDone(res);
            }
        };

        if (isChanged)
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UINoticeDialog>();
            dialog.onQuit += onCancel;
            dialog.Configure(
                "project_save_changed_params".Localize(),
                new DialogInputCallback(action => {
                    if (action == "Return_OK")
                    {
                        OpenSaveDialog(() => callback(SaveCodeStatus.Saved), onCancel);
                    }
                    else //if (action == "Return_NO")
                    {
                        callback(SaveCodeStatus.Discard);
                    }
                }));
            dialog.OpenDialog();
        }
        else
        {
            callback(SaveCodeStatus.Unchanged);
        }
    }

    public void SaveAs(Action onSaved)
    {
        OpenSaveDialog(onSaved, null);
    }

    private void OpenSaveDialog(Action onSaved, Action onCancel)
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditInputDialog>();
        var config = new UIEditInputDialogConfig {
            title = "ui_dialog_save_project_title",
            content = currentProjectName,
        };
        dialog.onQuit += onCancel;
        dialog.Configure(
            config, 
            new DialogInputCallback(name => {
                SaveAndSynchro(name.TrimEnd(), onSaved, onCancel);
            }),
            CreateProjectNameValidator());
        dialog.OpenDialog();
    }

    public void Save(Action<bool> onSaved)
    {
        Action<bool> callback = (res) => {
            if (onSaved != null)
            {
                onSaved(res);
            }
        };

        if (isChanged)
        {
            if (currentProjectName != "")
            {
                SaveAndSynchro(currentProjectName, () => callback(true), null);
            }
            else
            {
                SaveAs(() => onSaved(true));
            }
        }
        else
        {
            callback(false);
        }
    }

    public abstract bool isChanged { get; }

    protected abstract string currentProjectName { get; }

    protected abstract IDialogInputValidator CreateProjectNameValidator();

    protected abstract void SaveAndSynchro(string name, Action onSaved, Action onSaveError);
}
