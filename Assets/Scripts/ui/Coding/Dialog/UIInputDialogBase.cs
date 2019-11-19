using System;
using UnityEngine;
using System.Collections;

public abstract class UIInputDialogBase : PopupController
{
    // close the dialog without input callback
    public event Action onQuit;

    public Action onClosed { get; set; }

    public virtual void Init()
    {
    }

    public abstract UIDialog dialogType { get; }

    public virtual void OpenDialog()
    {
    }

    public virtual void CloseDialog()
    {
        if (onClosed != null)
        {
            onClosed();
        }

        Close();
    }

    public override void OnCloseButton()
    {
        CloseDialog();

        if (onQuit != null)
        {
            onQuit();
        }
    }
}
