using UnityEngine;
using System.Collections;

public class ScreenDebug : MonoBehaviour {
    static Queue g_DebugInfo = Queue.Synchronized(new Queue());
    int m_Line;

	GUIStyle m_Type;
	int m_fontSize = 30;

	public static void ScreenPrint(string info, bool screenPrint = false)
    {
        print(info);
        if(screenPrint)
        {
            g_DebugInfo.Enqueue(info);
        }
    }

	// Use this for initialization
	void Start () {
        m_Line = -1;
		m_Type = new GUIStyle();
		m_Type.fontSize = m_fontSize;
		m_Type.normal.textColor = Color.yellow;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(-1 != m_Line)
            {
                ++m_Line;
                if(m_Line > g_DebugInfo.Count)
                {
                    m_Line = -1;
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(-1 == m_Line)
            {
                m_Line = g_DebugInfo.Count;
            }
            --m_Line;
            if(m_Line <= 1)
            {
                m_Line = 1;
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (-1 != m_Line)
            {
                ++m_Line;
                if (m_Line > g_DebugInfo.Count)
                {
                    m_Line = -1;
                }
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (-1 == m_Line)
            {
                m_Line = g_DebugInfo.Count;
            }
            --m_Line;
            if (m_Line <= 1)
            {
                m_Line = 1;
            }
        }
    }

    void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height/2-150, 100, 100), "↑"))
		{
			if (-1 != m_Line)
			{
				++m_Line;
				if (m_Line > g_DebugInfo.Count)
				{
					m_Line = -1;
				}
			}
		}
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 50, 100, 100), "Clear"))
		{
			g_DebugInfo.Clear();
			m_Line = -1;
        }
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height/2+50, 100, 100), "↓"))
		{
			if (-1 == m_Line)
			{
				m_Line = g_DebugInfo.Count;
			}
			--m_Line;
			if (m_Line <= 1)
			{
				m_Line = 1;
			}
		}
        int mMaxLine = Screen.height / m_fontSize + 1;

        string mInfo = "";
        int mCount = 0;
        object[] temp = g_DebugInfo.ToArray();
        foreach (var item in temp)
        {
            if(0 != mCount)
            {
                mInfo = "\n" + mInfo;
            }
            mInfo = item + mInfo;
            ++mCount;
            if(-1 != m_Line)
            {
                if(mCount >= m_Line)
                {
                    break;
                }
            }
        }
        int lastPos = 0;
        for(int i = 0; i < mMaxLine; ++i)
        {
            int pos = mInfo.IndexOf("\n", lastPos);
            if(-1 == pos)
            {
                lastPos = 0;
                break;
            }
            else
            {
                lastPos = pos + 1;
            }
        }
        if(0 != lastPos)
        {
            mInfo = mInfo.Remove(lastPos);
        }
        if (0 != mInfo.Length)
        {
            GUI.Label(new Rect(10, 10, Screen.width - 20, 44), mInfo, m_Type);
        }
    }
}
