using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class UIWidget
    : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IPointerEnterHandler
    , IPointerExitHandler
    , IPointerClickHandler
{
    private Transform m_trans;

    protected virtual void Awake()
    {
        m_trans = transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!HandlePointerDown(eventData) && m_trans.parent)
        {
            ExecuteEvents.ExecuteHierarchy(m_trans.parent.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!HandlePointerUp(eventData) && m_trans.parent)
        {
            ExecuteEvents.ExecuteHierarchy(m_trans.parent.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HandlePointerEnter(eventData) && m_trans.parent)
        {
            ExecuteEvents.ExecuteHierarchy(m_trans.parent.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!HandlePointerExit(eventData) && m_trans.parent)
        {
            ExecuteEvents.ExecuteHierarchy(m_trans.parent.gameObject, eventData, ExecuteEvents.pointerExitHandler);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!HandlePointerClick(eventData) && m_trans.parent)
        {
            ExecuteEvents.ExecuteHierarchy(m_trans.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }
    }

    #region routable events

    protected virtual bool HandlePointerDown(PointerEventData eventData)
    {
        return true;
    }

    protected virtual bool HandlePointerUp(PointerEventData eventData)
    {
        return true;
    }

    protected virtual bool HandlePointerEnter(PointerEventData eventData)
    {
        return true;
    }

    protected virtual bool HandlePointerExit(PointerEventData eventData)
    {
        return true;
    }

    protected virtual bool HandlePointerClick(PointerEventData eventData)
    {
        return true;
    }

    #endregion
}
