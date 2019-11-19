using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeTouchInput : BaseInput
{
    public bool fakingTouch;
    public Vector2 fakeTouchPos;

    private Touch? m_fixedTouch;
    private Touch? m_mouseTouch;

    public override int touchCount
    {
        get
        {
            int touchCount = 0;
            if (m_fixedTouch != null)
            {
                ++touchCount;
            }
            if (m_mouseTouch != null)
            {
                ++touchCount;
            }
            return touchCount;
        }
    }

    public override Touch GetTouch(int index)
    {
        if (index == 0)
        {
            return m_fixedTouch.Value;
        }
        return m_mouseTouch.Value;
    }

    void Update()
    {
        if (fakingTouch)
        {
            if (m_fixedTouch == null)
            {
                m_fixedTouch = CreateTouch(fakeTouchPos, 0);
            }
            else
            {
                var touch = m_fixedTouch.Value;
                touch.phase = TouchPhase.Stationary;
                m_fixedTouch = touch;

                if (Input.GetMouseButton(0))
                {
                    if (m_mouseTouch == null)
                    {
                        m_mouseTouch = CreateTouch(Input.mousePosition, 1);
                    }
                    else
                    {
                        touch = m_mouseTouch.Value;
                        if (touch.position != Input.mousePosition.xy())
                        {
                            touch.phase = TouchPhase.Moved;
                            touch.position = Input.mousePosition;
                        }
                        else
                        {
                            touch.phase = TouchPhase.Stationary;
                        }
                        m_mouseTouch = touch;
                    }
                }
                else if (m_mouseTouch != null)
                {
                    touch = m_mouseTouch.Value;
                    if (touch.phase != TouchPhase.Ended)
                    {
                        touch.phase = TouchPhase.Ended;
                        m_mouseTouch = touch;
                    }
                    else
                    {
                        m_mouseTouch = null;
                    }
                }
            }
        }
        else
        {
            EndTouch(ref m_fixedTouch);
            EndTouch(ref m_mouseTouch);
        }
    }

    void EndTouch(ref Touch? touch)
    {
        if (touch != null)
        {
            if (touch.Value.phase != TouchPhase.Ended)
            {
                var tmpTouch = touch.Value;
                tmpTouch.phase = TouchPhase.Ended;
                touch = tmpTouch;
            }
            else
            {
                touch = null;
            }
        }
    }

    Touch CreateTouch(Vector2 position, int fingerId)
    {
        Touch touch = new Touch();
        touch.position = position;
        touch.phase = TouchPhase.Began;
        touch.type = TouchType.Direct;
        touch.fingerId = fingerId;

        return touch;
    }
}