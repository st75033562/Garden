using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// tween all children's color based on the setting of the button
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonColorEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Button m_button;
    private Graphic[] m_childGraphics;
    private bool m_isPressed;
    private bool m_isInside;
    private bool m_interactable = true;

    void Awake()
    {
        m_childGraphics = GetComponentsInChildren<Graphic>(true).Except(button.targetGraphic).ToArray();
    }

    private Button button
    {
        get
        {
            if (!m_button)
            {
                m_button = GetComponent<Button>();
                m_interactable = m_button.interactable;
            }
            return m_button;
        }
    }

    void Start()
    {
        UpdateColor();
    }

    void OnEnable()
    {
        UpdateColor();
    }

    public bool interactable
    {
        get { return m_interactable; }
        set
        {
            if (interactable != value)
            {
                button.interactable = value;
                m_interactable = value;
                UpdateColor();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // button only accepts left click
        if (eventData.button != PointerEventData.InputButton.Left || !m_button.enabled)
        {
            return;
        }

        m_isPressed = true;
        UpdateColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // button only accepts left click
        if (eventData.button != PointerEventData.InputButton.Left || !m_button.enabled)
        {
            return;
        }

        m_isPressed = false;
        UpdateColor();
    }

    private void FadeTo(ref ColorBlock block, Color color)
    {
        if (m_childGraphics == null) { return; }

        foreach (var child in m_childGraphics)
        {
            child.CrossFadeColor(color * block.colorMultiplier, block.fadeDuration, true, true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_isInside = true;
        UpdateColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_isInside = false;
        UpdateColor();
    }

    private void UpdateColor()
    {
        var colors = m_button.colors;
        Color targetColor;
        if (!interactable)
        {
            targetColor = colors.disabledColor;
        }
        else if (m_isPressed)
        {
            targetColor = colors.pressedColor;
        }
        else if (m_isInside)
        {
            targetColor = colors.highlightedColor;
        }
        else
        {
            targetColor = colors.normalColor;
        }

        FadeTo(ref colors, targetColor);
    }

    void Update()
    {
        if (m_interactable != m_button.interactable)
        {
            m_interactable = m_button.interactable;
            UpdateColor();
        }
    }
}
