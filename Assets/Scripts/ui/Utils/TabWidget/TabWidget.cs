using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class TabWidget : UIBehaviour
{
    [Serializable]
    public class TabEvent : UnityEvent<int> { }

    /// <summary>
    /// fired when user requests activating a new tab
    /// if null, then the new tab is activated immediately, otherwise client is responsible for activating the tab
    /// NOTE: onTabChanging won't be fired if client changes the active tab by calling activeTabIndex
    /// </summary>
    public event Action<int> onTabChanging;

    [SerializeField]
    private TabEvent m_tabChanged;

    // contains all tab buttons, derived class is responsible for populating the list
    protected readonly List<TabButtonWidget> m_tabButtons = new List<TabButtonWidget>();
    protected int m_activeTabIndex = -1;

    /// <summary>
    /// the active tab index, initially is -1
    /// </summary>
    public int activeTabIndex
    {
        get { return m_activeTabIndex; }
        set
        {
            if (value < 0 || value >= m_tabButtons.Count)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            if (m_activeTabIndex == value)
            {
                return;
            }

            m_activeTabIndex = value;
            for (int i = 0; i < m_tabButtons.Count; ++i)
            {
                m_tabButtons[i].isOn = i == value;
            }

            onTabChanged.Invoke(value);
        }
    }

    public void OnClickTabButton(TabButtonWidget button)
    {
        var tabIndex = m_tabButtons.IndexOf(button);
        if (tabIndex == m_activeTabIndex)
        {
            return;
        }

        if (onTabChanging != null)
        {
            onTabChanging(tabIndex);
        }
        else
        {
            activeTabIndex = tabIndex;
        }
    }

    /// <summary>
    /// fired after the new tab is activated
    /// </summary>
    public TabEvent onTabChanged
    {
        get { return m_tabChanged; }
    }
}
