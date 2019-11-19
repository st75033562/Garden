using UnityEngine;
using UnityEngine.UI;

public class UIARMarkerData
{
    public Sprite sprite;
    public int markerId;
}

public class UIARMarkerItem : ScrollableCell
{
    public Text m_idText;
    public Image m_markerImage;

    public override void ConfigureCellData()
    {
        m_markerImage.sprite = data.sprite;
        m_idText.text = data.markerId.ToString();
    }

    public Sprite markerSprite
    {
        get { return m_markerImage.sprite; }
        set { m_markerImage.sprite = value; }
    }

    public UIARMarkerData data
    {
        get { return (UIARMarkerData)dataObject; }
    }
}
