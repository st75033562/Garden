using System.Linq;
using UnityEngine;

public class UIARMarkerDialog : UIInputDialogBase
{
    public Sprite[] m_markerSprites;
    public ScrollableAreaController m_scrollController;
    private IDialogInputCallback m_callback;

    public void Configure(IDialogInputCallback callback)
    {
        m_callback = callback;
        m_scrollController.InitializeWithData(
            m_markerSprites.Select((x, i) => new UIARMarkerData {
                sprite = x,
                markerId = i,
            }).ToArray());
    }

    public void SelectMarker(UIARMarkerItem item)
    {
        if (m_callback != null)
        {
            m_callback.InputCallBack(item.data.markerId.ToString());
        }
        CloseDialog();
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIARMarkerDialog; }
    }
}
