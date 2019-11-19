using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButtonToggle : MonoBehaviour
{
    public Button m_offButton;
    public Button m_onButton;

    public delegate bool BeforeChangingStateHandler(bool isOn);

    [Serializable]
    public class StateEvent : UnityEvent<bool> { }

    public StateEvent m_ValueChanged;

    void Start()
    {
        m_offButton.onClick.AddListener(GetClickListener(true));
        m_onButton.onClick.AddListener(GetClickListener(false));
    }

    public StateEvent onValueChanged
    {
        get { return m_ValueChanged; }
    }

    private UnityAction GetClickListener(bool newState)
    {
        return () => {
            if (beforeChangingState != null && !beforeChangingState(newState))
            {
                return;
            }
            isOn = newState;
        };
    }

    /// <summary>
    /// event triggered before changing state through UI, return false to cancel the change
    /// </summary>
    public BeforeChangingStateHandler beforeChangingState { get; set; }

    public bool interactable
    {
        get { return m_offButton.interactable; }
        set
        {
            m_offButton.interactable = value;
            m_onButton.interactable = value;
        }
    }

    public bool isOn
    {
        get
        {
            return m_onButton.gameObject.activeSelf;
        }
        set
        {
            if (isOn != value)
            {
                m_offButton.gameObject.SetActive(!value);
                m_onButton.gameObject.SetActive(value);
                if (m_ValueChanged != null)
                {
                    m_ValueChanged.Invoke(value);
                }
            }
        }
    }
}
