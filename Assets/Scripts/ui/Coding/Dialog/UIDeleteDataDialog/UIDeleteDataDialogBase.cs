using UnityEngine;
using UnityEngine.UI;

public abstract class UIDeleteDataDialogBase : UIInputDialogBase
{
    public LayoutElement m_ScrollViewLayout;
    public float m_MaxScrollViewHeight;
    public VerticalLayoutGroup m_ContentLayout;
    public RectTransform m_ContentTrans;

    protected void RemoveItem(GameObject item)
    {
        Destroy(item.gameObject);
        item.transform.SetParent(null);
        Layout();

        if (m_ContentTrans.childCount == 0)
        {
            CloseDialog();
        }
    }

    protected void Layout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentTrans);
        float preferredHeight = LayoutUtility.GetPreferredHeight(m_ContentTrans);
        m_ScrollViewLayout.preferredHeight = Mathf.Min(preferredHeight, m_MaxScrollViewHeight);
    }
}
