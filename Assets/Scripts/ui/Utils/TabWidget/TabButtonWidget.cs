using UnityEngine;

public abstract class TabButtonWidget : MonoBehaviour
{
    // used internally to update the visual state of the widget
    public abstract bool isOn
    {
        get;
        set;
    }
}
