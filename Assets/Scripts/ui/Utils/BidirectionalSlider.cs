using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
public class BidirectionalSlider : MonoBehaviour
{
    public RectTransform m_leftFill;
    public RectTransform m_rightFill;

    private Slider m_slider;

    void Start()
    {
        m_slider = GetComponent<Slider>();
        m_slider.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(m_slider.value);
    }

    void OnValueChanged(float value)
    {
        value = m_slider.normalizedValue * 2.0f - 1.0f;
        SetWidth(m_leftFill, -value);
        SetWidth(m_rightFill, value);
    }

    void SetWidth(RectTransform fillRect, float value)
    {
        if (fillRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fillRect);

            var width = Mathf.Max(0, value) * (fillRect.parent as RectTransform).rect.width;
            fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}
