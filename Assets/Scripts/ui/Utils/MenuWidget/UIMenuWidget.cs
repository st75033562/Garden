using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuWidget : MonoBehaviour
{
    public IntUnityEvent m_onItemClicked;
    public RectTransform m_container;
    public UIMenuItemWidget m_itemTemplate;
    public GameObject m_seperator;
    private readonly List<UIMenuItemWidget> m_items = new List<UIMenuItemWidget>();

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Open(Transform anchor)
    {
        SetPosition(anchor.position);
        Open();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector3 pos)
    {
        m_container.position = pos;
    }

    public virtual void SetOptions(IList<string> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException("options");
        }

        m_items.Clear();

        int widgetIndex = 0;
        for (int i = 0; i < options.Count; ++i)
        {
            UIMenuItemWidget item;
            if (widgetIndex < m_container.childCount)
            {
                item = m_container.GetChild(widgetIndex).GetComponent<UIMenuItemWidget>();
            }
            else
            {
                var go = Instantiate(m_itemTemplate.gameObject, m_container);
                go.SetActive(true);
                item = go.GetComponent<UIMenuItemWidget>();
                item.itemIndex = i;
                item.onClick = OnClick;
            }
            item.text = options[i];
            ++widgetIndex;

            m_items.Add(item);

            if (m_seperator && i < options.Count - 1)
            {
                if (widgetIndex >= m_container.childCount)
                {
                    var go = Instantiate(m_seperator, m_container);
                    go.SetActive(true);
                }
                ++widgetIndex;
            }
        }

        for (int j = m_container.childCount - 1; j >= widgetIndex; --j)
        {
            var child = m_container.GetChild(j);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    public IntUnityEvent onItemClicked { get { return m_onItemClicked; } }

    public UIMenuItemWidget GetItem(int index)
    {
        return m_items[index];
    }

    public T GetItem<T>(int index) where T : UIMenuItemWidget
    {
        return (T)GetItem(index);
    }

    public int itemCount
    {
        get { return m_items.Count; }
    }

    protected virtual void OnClick(UIMenuItemWidget menuItem)
    {
        if (m_onItemClicked != null)
        {
            m_onItemClicked.Invoke(menuItem.itemIndex);
        }

        Close();
    }
}
