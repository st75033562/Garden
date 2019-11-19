using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTabWidget : TabWidget
{
    public SimpleTabButtonWidget m_buttonTemplate;
    public RectTransform m_buttonContainer;

    /// <summary>
    /// initialize the tab widget with given buttons
    /// the active tab is not set after the invocation
    /// </summary>
    /// <param name="buttons"></param>
    public void SetTabs(IEnumerable<string> buttons)
    {
        if (buttons == null)
        {
            throw new ArgumentNullException("buttons");
        }

        m_activeTabIndex = -1;
        m_tabButtons.Clear();

        foreach (var buttonText in buttons)
        {
            var instance = Instantiate(m_buttonTemplate.gameObject, m_buttonContainer).GetComponent<SimpleTabButtonWidget>();
            instance.gameObject.SetActive(true);
            m_tabButtons.Add(instance);

            instance.text = buttonText;
        }
    }
}
