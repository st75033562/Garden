using UnityEngine.UI;

public class PopupNotice : PopupController
{
    public Graphic mask;

    public bool modal
    {
        get { return mask.enabled; }
        set { mask.enabled = value; }
    }
}
