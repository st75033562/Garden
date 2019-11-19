using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StackUIBase : MonoBehaviour
{
	static readonly List<StackUIBase> s_Order = new List<StackUIBase>();

    private bool m_hidden;

	public virtual void OpenWindow(bool isPush)
	{
        m_hidden = false;
		gameObject.SetActive(true);
	}

	public virtual void CloseWindow(bool isPush)
	{
        m_hidden = true;
		gameObject.SetActive(false);
	}

	public static void Push(StackUIBase obj, bool hidePrev = true)
	{
        if (0 != s_Order.Count && hidePrev)
		{
			s_Order.Last().CloseWindow(true);
		}
		obj.OpenWindow(true);
		s_Order.Add(obj);
	}

	public static void Pop()
	{
        if (0 != s_Order.Count)
		{
			s_Order.Last().CloseWindow(false);
            s_Order.RemoveAt(s_Order.Count - 1);
		}
		if (0 != s_Order.Count)
		{
            var cur = s_Order.Last();
            if (cur.m_hidden)
            {
                cur.OpenWindow(false);
            }
		}
	}

    protected static void Pop(StackUIBase ui)
    {
        if (s_Order.Count != 0 && s_Order.Last() == ui)
        {
            Pop();
        }
    }

	public static void Clear()
	{
		s_Order.Clear();
	}

	protected virtual void OnDestroy()
	{
        if (ApplicationEvent.isQuitting)
        {
            return;
        }

        var index = s_Order.IndexOf(this);
        if (index != -1)
        {
            if (index == s_Order.Count - 1)
            {
                Pop();
            }
            else
            {
                s_Order.RemoveAt(index);
            }
        }
	}
}
