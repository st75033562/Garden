using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// continuously fire onPressed event
/// </summary>
public class UIRepeatButton
    : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IPointerClickHandler
{
    public UnityEvent onPressed;

    /// <summary>
    /// onClicked will only be fired once
    /// </summary>
    public UnityEvent onClicked;

    public float delay = 0.5f;
    public float interval = 0.1f;

    protected int pressCount
    {
        get;
        private set;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(CheckPress());
    }

    private IEnumerator CheckPress()
    {
        pressCount = 1;
        FirePressEvent();

        yield return new WaitForSeconds(delay);
        for (;;)
        {
            ++pressCount;
            FirePressEvent();

            yield return new WaitForSeconds(interval);
        }
    }

    protected virtual void FirePressEvent()
    {
        if (onPressed != null)
        {
            onPressed.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClicked != null)
        {
            onClicked.Invoke();
        }
    }
}
