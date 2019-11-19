using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : ScrollCell, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public bool drag = true;
    public void OnBeginDrag(PointerEventData eventData) {
        if(!drag)
            return;
        SetDraggedPosition(eventData , true);
        gameObject.GetComponent<RectTransform>().SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) {
        if(!drag)
            return;
        SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if(!drag)
            return;
        SetDraggedPosition(eventData);

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData , raycastResults);
        foreach (RaycastResult r in raycastResults)
        {
            EndDragRaycast(r.gameObject);
            
        }
    }


    private Vector3 offset;
    private void SetDraggedPosition(PointerEventData eventData ,bool isBegin = false) {
        
        var rt = gameObject.GetComponent<RectTransform>();

        Vector3 globalMousePos;
        
        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out globalMousePos)) {
            if(isBegin) {
                offset = rt.position - globalMousePos;
            } else {
                rt.position = globalMousePos + offset;
            }
            
        }
    }

    public virtual void EndDragRaycast(GameObject go) { }
}
