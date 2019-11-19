using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollableInputField : InputField
{
    public float m_scrollThresholdTime = 0.25f;

    private ScrollRect m_scrollRect;
    private int m_lastCaretPosition = -1;
    private bool m_heightUpdated;
    private RectTransform m_rectTransform;
    private bool m_scrolling;
    private Coroutine m_coScrollTimer;

    protected override void Awake()
    {
        base.Awake();

        m_scrollRect = GetComponentInParent<ScrollRect>();
        if (!m_scrollRect)
        {
            Debug.LogError("ScrollRect not found in parent");
        }
        m_rectTransform = GetComponent<RectTransform>();
        onValueChanged.AddListener(UpdateLabelHeight);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_scrolling = false;
        StopScrollTimer();
    }

    void UpdateLabelHeight(string text)
    {
        if (!m_rectTransform || !textComponent) { return; }

        var viewportHeight = m_scrollRect.viewport.rect.height;
        if (viewportHeight == 0) { return; }

        var maxWidth = m_rectTransform.rect.width;
        var textGenSettings = textComponent.GetGenerationSettings(new Vector2(maxWidth, 0));
        textGenSettings.scaleFactor = 1.0f;
        var textHeight = textComponent.cachedTextGeneratorForLayout.GetPreferredHeight(text, textGenSettings);
        var height = Mathf.Max(textHeight, viewportHeight);

        m_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        ClampContent();

        m_heightUpdated = true;
    }

    protected override void LateUpdate()
    {
        if (!m_heightUpdated)
        {
            UpdateLabelHeight(text);
        }

        base.LateUpdate();

        if (isFocused && caretPosition != m_lastCaretPosition && cachedInputTextGenerator.lineCount > 0)
        {
            m_lastCaretPosition = caretPosition;

            float lineHeight = cachedInputTextGenerator.lines[0].height;
            float topY = GetCaretLineTopY();

            // check if the line with caret is visible in viewport
            float topYInContent = topY + textComponent.rectTransform.localPosition.y;
            float topYInViewport = topYInContent + m_scrollRect.content.localPosition.y;
            if (topYInViewport > 0)
            {
                UpdateScrollbarPosition(topYInContent - m_scrollRect.viewport.rect.height);
            }
            else if (topYInViewport - lineHeight < -m_scrollRect.viewport.rect.height)
            {
                UpdateScrollbarPosition(topYInContent - lineHeight);
            }
            else
            {
                ClampContent();
            }
        }
        else if (!isFocused && m_lastCaretPosition != -1)
        {
            m_lastCaretPosition = -1;
        }
    }

    float GetCaretLineTopY()
    {
        float topY = float.PositiveInfinity;
        var lines = cachedInputTextGenerator.lines;
        for (int i = 0; i < lines.Count - 1; ++i)
        {
            if (m_lastCaretPosition < lines[i + 1].startCharIdx)
            {
                topY = lines[i].topY;
                break;
            }
        }

        if (topY == float.PositiveInfinity)
        {
            topY = lines[lines.Count - 1].topY;
        }
        return topY / textComponent.canvas.scaleFactor;
    }

    void UpdateScrollbarPosition(float bottomYInContent)
    {
        float viewportHeight = m_scrollRect.viewport.rect.height;
        float scrollHeight = m_scrollRect.content.rect.height - viewportHeight;
        float t = scrollHeight >= 1e-3f ? (m_scrollRect.content.rect.height + bottomYInContent) / scrollHeight : 0.0f;
        m_scrollRect.verticalNormalizedPosition = t;
        // manually update the vertical bar otherwise the scrollbar will be updated in next LateUpdate
        m_scrollRect.verticalScrollbar.value = t;
    }

    void ClampContent()
    {
        // clamp the content in the viewport
        float contentBottomY = -m_rectTransform.rect.height + m_rectTransform.localPosition.y;
        if (contentBottomY > -m_scrollRect.viewport.rect.height + 0.01f)
        {
            m_scrollRect.verticalNormalizedPosition = 0.0f;
            m_scrollRect.verticalScrollbar.value = 0.0f;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (Application.isMobilePlatform)
        {
            m_scrolling = true;
        }

        if (m_scrolling)
        {
            m_scrollRect.OnBeginDrag(eventData);
        }
        else
        {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (m_scrolling)
        {
            m_scrollRect.OnDrag(eventData);
        }
        else
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (m_scrolling)
        {
            m_scrollRect.OnEndDrag(eventData);
            m_scrolling = false;
        }
        else
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (Application.isMobilePlatform)
        {
            m_coScrollTimer = StartCoroutine(ScrollTimer());
        }
        else
        {
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        StopScrollTimer();
        if (!m_scrolling)
        {
            if (Application.isMobilePlatform)
            {
                base.OnPointerDown(eventData);
            }
            base.OnPointerClick(eventData);
        }
        else
        {
            StartCoroutine(ResetScrolling());
        }
    }

    IEnumerator ScrollTimer()
    {
        yield return new WaitForSeconds(m_scrollThresholdTime);
        m_scrolling = true;
    }

    void StopScrollTimer()
    {
        if (m_coScrollTimer != null)
        {
            StopCoroutine(m_coScrollTimer);
            m_coScrollTimer = null;
        }
    }

    IEnumerator ResetScrolling()
    {
        yield return null;
        m_scrolling = false;
    }
}
