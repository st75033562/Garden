using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISwitchButton : UIBehaviour
{
    [SerializeField]
    private UnityEvent m_onAnimationBegin;

    [SerializeField]
    private UnityEvent m_onAnimationEnd;

    [SerializeField]
    private IntUnityEvent m_onValueChanged;

    public event Func<int, bool> onValueChanging;

    public Image m_indicator;
    public float m_indicatorPadding;

    // use to determine the position and size of the indicator
    public RectTransform[] m_options;

    [SerializeField]
    private float m_animTime = 0.15f;

    [SerializeField]
    private int m_option;

    private bool m_interactable = true;

    protected override void Start()
    {
        base.Start();

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        AnimateIndicator(0.0f);

        interactable = m_interactable;
    }

    public int option
    {
        get { return m_option; }
        set
        {
            if (value < 0 || value >= m_options.Length)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            if (m_option == value)
            {
                return;
            }

            if (onValueChanging != null)
            {
                foreach (var listener in onValueChanging.GetInvocationList())
                {
                    if (!((Func<int, bool>)listener).Invoke(value))
                    {
                        return;
                    }
                }
            }

            m_option = value;
            if (m_onValueChanged != null)
            {
                m_onValueChanged.Invoke(value);
            }
            AnimateIndicator(m_animTime);
        }
    }

    public bool interactable
    {
        get { return m_interactable; }
        set
        {
            m_interactable = value;
            SetOptionInteractable(value);
        }
    }

    private void SetOptionInteractable(bool interactable)
    {
        foreach (var optionTrans in m_options)
        {
            optionTrans.GetComponent<Button>().interactable = interactable;
        }
    }

    public float animationTime
    {
        get { return m_animTime; }
    }

    private void AnimateIndicator(float time)
    {
        if (m_onAnimationBegin != null)
        {
            m_onAnimationBegin.Invoke();
        }

        var newOption = m_options[m_option];
        var newPos = newOption.rect.center.x + newOption.localPosition.x;
        m_indicator.rectTransform.DOLocalMoveX(newPos, time);

        var newSize = m_indicator.rectTransform.sizeDelta;
        newSize.x = newOption.rect.width + m_indicatorPadding;
        m_indicator.rectTransform.DOSizeDelta(newSize, time)
            .OnComplete(() => {
                if (m_onAnimationEnd != null)
                {
                    m_onAnimationEnd.Invoke();
                }
            });
    }

    public UnityEvent onAnimationEnd { get { return m_onAnimationEnd; } }

    public UnityEvent onAnimationBegin { get { return m_onAnimationBegin; } }

    public IntUnityEvent onValueChanged { get { return m_onValueChanged; } }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (m_options != null)
        {
            m_option = Mathf.Clamp(m_option, 0, Mathf.Max(0, m_options.Length - 1));
        }
    }
#endif
}
