using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Selectable m_selectable;
    private Color m_highlightedColor;

    void Awake()
    {
        var colors = m_selectable.colors;
        m_highlightedColor = colors.highlightedColor;
        colors.highlightedColor = colors.normalColor;
        m_selectable.colors = colors;
    }

    void OnEnable()
    {
        var data = new PointerEventData(EventSystem.current);
        m_selectable.OnPointerUp(data);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var colors = m_selectable.colors;
        colors.highlightedColor = m_highlightedColor;
        m_selectable.colors = colors;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var colors = m_selectable.colors;
        colors.highlightedColor = colors.normalColor;
        m_selectable.colors = colors;
    }

    void Reset()
    {
        m_selectable = GetComponent<Selectable>();
    }
}
