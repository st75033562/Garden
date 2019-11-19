using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleColorOnClick : MonoBehaviour, IPointerClickHandler
{
    public Color color;
    public Graphic target;

    private bool m_clicked;
    private Color m_originalColor;
    private Button m_button;

    void Start()
    {
        m_originalColor = target.color;
        m_button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!m_button || m_button.interactable)
        {
            m_clicked = !m_clicked;
            UpdateColor();
        }
    }

    public void Restore()
    {
        m_clicked = false;
        UpdateColor();
    }

    private void UpdateColor()
    {
        target.color = m_clicked ? color : m_originalColor;
    }

    void Reset()
    {
        target = GetComponent<Graphic>();
        color = Color.white;
    }
}
