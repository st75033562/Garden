using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickDetector
{
    private int m_lastPointerId = int.MaxValue;
    private GameObject m_lastClickedObject;
    private float m_lastClickTime;
    private int m_clickCount;

    /// <summary>
    /// detect double clicking with pointer click event data
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns>true if double clicking is detected</returns>
    public bool Detect(PointerEventData eventData)
    {
        var detected = false;

        if (m_lastPointerId == eventData.pointerId && 
            m_lastClickedObject == eventData.pointerPress && 
            eventData.clickTime - m_lastClickTime < 0.3f)
        {
            ++m_clickCount;
            if (m_clickCount == 2)
            {
                m_clickCount = 0;
                detected = true;
            }
        }
        else
        {
            m_clickCount = 1;
        }
        m_lastPointerId = eventData.pointerId;
        m_lastClickedObject = eventData.pointerPress;
        m_lastClickTime = eventData.clickTime;

        return detected;
    }
}
