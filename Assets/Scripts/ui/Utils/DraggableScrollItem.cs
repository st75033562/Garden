using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableScrollItem 
    : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IInitializePotentialDragHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    [SerializeField]
    private ScrollRect m_scrollRect;

    [SerializeField]
    private UnityEvent m_beginDragEvent = new UnityEvent();

    [SerializeField]
    private UnityEvent m_dragEvent = new UnityEvent();

    [SerializeField]
    private UnityEvent m_endDragEvent = new UnityEvent();

    private const float BeginDragDelay = 0.15f;
    private static readonly float OrthoDragThreashold = Mathf.Cos(Mathf.Deg2Rad * 60.0f);

    // do not support multi touch
    private static float s_downTime;
    private static bool s_dragging;
    private static DraggableScrollItem s_draggingItem;

    public UnityEvent onBeginDrag
    {
        get { return m_beginDragEvent; }
    }

    public UnityEvent onDrag
    {
        get { return m_dragEvent; }
    }

    public UnityEvent onEndDrag
    {
        get { return m_endDragEvent; }
    }

    void OnDisable()
    {
        // disabled object won't receive events, so end the dragging
        if (!gameObject.activeInHierarchy)
        {
            ResetDragging();
        }
    }

    void OnDestroy()
    {
        ResetDragging();
    }

    private void ResetDragging()
    {
        if (s_draggingItem == this)
        {
            s_draggingItem = null;
            s_downTime = 0;
            if (s_dragging)
            {
                s_dragging = false;
                onEndDrag.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (s_draggingItem == null)
        {
            s_draggingItem = this;
            s_downTime = Time.realtimeSinceStartup;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetDragging();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (m_scrollRect)
        {
            m_scrollRect.OnInitializePotentialDrag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (s_draggingItem == this)
        {
            bool beginDrag = false;

            // if the scroll rect is not freely scrollable, then we can treat
            // the move which is orthogonal to the scroll direction as dragging.
            if (m_scrollRect.horizontal && !m_scrollRect.vertical)
            {
                beginDrag = Mathf.Abs(eventData.delta.normalized.y) >= OrthoDragThreashold;
            }
            else if (!m_scrollRect.horizontal && m_scrollRect.vertical)
            {
                beginDrag = Mathf.Abs(eventData.delta.normalized.x) >= OrthoDragThreashold;
            }

            if (s_downTime > 0 && !beginDrag)
            {
                beginDrag = Time.realtimeSinceStartup - s_downTime >= BeginDragDelay;
                s_downTime = 0;
            }

            if (beginDrag)
            {
                s_dragging = true;
                onBeginDrag.Invoke();
            }
        }


        if (!s_dragging && m_scrollRect)
        {
            m_scrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!s_dragging && m_scrollRect)
        {
            m_scrollRect.OnDrag(eventData);
        }
        else if (s_dragging && s_draggingItem == this)
        {
            onDrag.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!s_dragging && m_scrollRect)
        {
            m_scrollRect.OnEndDrag(eventData);
        }
        else if (s_dragging && s_draggingItem == this)
        {
            onEndDrag.Invoke();

            s_dragging = false;
            ResetDragging();
        }
    }

    void Reset()
    {
        m_scrollRect = GetComponentInParent<ScrollRect>();
    }
}
