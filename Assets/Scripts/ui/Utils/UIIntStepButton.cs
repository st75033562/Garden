using System;
using UnityEngine.Events;

/// <summary>
/// NOTE: unity does not serialize generic events, so we duplicate implementation for each type
/// </summary>
public class UIIntStepButton : UIRepeatButton
{
    [Serializable]
    public class OnValueChangedEvent : UnityEvent<int> { }

    /// <summary>
    /// NOTE: you need to use Dynamic event handler
    /// </summary>
    public OnValueChangedEvent onValueChanged;

    public int valueOnFirstPress;
    public int valueOnContinuousPress;

    protected override void FirePressEvent()
    {
        base.FirePressEvent();

        if (onValueChanged != null)
        {
            onValueChanged.Invoke(pressCount == 1 ? valueOnFirstPress : valueOnContinuousPress);
        }
    }
}