using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SingleTouchEventTrigger : EventTrigger
{
    private int m_pointerId = int.MaxValue;

    public override void OnDrag(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (m_pointerId == int.MaxValue)
        {
            m_pointerId = eventData.pointerId;
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnPointerUp(eventData);
            StartCoroutine(ResetPointerId());
        }
    }

    private IEnumerator ResetPointerId()
    {
        yield return new WaitForEndOfFrame();
        m_pointerId = int.MaxValue;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnPointerClick(eventData);
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnInitializePotentialDrag(eventData);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (m_pointerId == eventData.pointerId)
        {
            base.OnEndDrag(eventData);
        }
    }
}
