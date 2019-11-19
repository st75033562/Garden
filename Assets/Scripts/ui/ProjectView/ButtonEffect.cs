using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Graphics = UnityEngine.UI.Graphic;

[ExecuteInEditMode]
public class ButtonEffect : UIWidget
{
    public Color normalColor = Color.white;
    public Color pressedColor = new Color(0.784f, 0.784f, 0.784f);
    public float fadeDuration = 0.1f;

    private Graphics m_graphic; 

    protected override void Awake()
    {
        base.Awake();

        m_graphic = GetComponent<Graphics>();
        if (m_graphic)
        {
            m_graphic.color = normalColor;
        }
    }

    protected override bool HandlePointerDown(PointerEventData eventData)
    {
        if (m_graphic)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateColor(pressedColor));
        }
        return false;
    }

    private IEnumerator UpdateColor(Color targetColor)
    {
        Color startColor = m_graphic.color;
        float time = 0;
        float duration = fadeDuration;
        if (duration <= 0)
        {
            m_graphic.color = targetColor;
            yield break;
        }
        while (time < fadeDuration)
        {
            m_graphic.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
            time += Time.deltaTime;
        }
        m_graphic.color = targetColor;
    }

    protected override bool HandlePointerUp(PointerEventData eventData)
    {
        if (m_graphic)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateColor(normalColor));
        }
        return false;
    }

    protected override bool HandlePointerEnter(PointerEventData eventData)
    {
        return false;
    }

    protected override bool HandlePointerExit(PointerEventData eventData)
    {
        return false;
    }

    protected override bool HandlePointerClick(PointerEventData eventData)
    {
        return false;
    }
}
