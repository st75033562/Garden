using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform m_uiRoot;

    private Vector3 m_startDragPosition;
    private Vector3 m_startInputWorldPos;
    private Canvas m_canvas;
    private RectTransform m_canvasRect;

    void Awake()
    {
        m_canvas = m_uiRoot.GetComponentInParent<Canvas>();
        m_canvasRect = m_canvas.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_startDragPosition = m_uiRoot.position;
        m_startInputWorldPos = GetWorldPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_uiRoot.position = m_startDragPosition + GetWorldPosition(eventData.position) - m_startInputWorldPos;
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasRect, screenPos, m_canvas.worldCamera, out worldPos);
        return worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
