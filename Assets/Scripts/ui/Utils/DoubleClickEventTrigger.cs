using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DoubleClickEventTrigger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] private UnityEvent m_onClicked = new UnityEvent();

    private readonly DoubleClickDetector m_detector = new DoubleClickDetector();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_detector.Detect(eventData))
        {
            m_onClicked.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // for intercepting pointer click event
    }

    public UnityEvent onClicked
    {
        get { return m_onClicked; }
    }
}
