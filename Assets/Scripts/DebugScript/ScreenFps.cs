using UnityEngine;
using System.Collections;

public class ScreenFps : MonoBehaviour
{
	int m_fps;
	float m_Time;
	int m_fpsToUI;

	GUIStyle m_fpsType;
	// Use this for initialization
	void Start () {
		m_fps = 0;
		m_Time = 0.0f;
		m_fpsToUI = 0;
		m_fpsType = new GUIStyle();
		m_fpsType.fontSize = 30;
		m_fpsType.normal.textColor = Color.yellow;
    }
	
	// Update is called once per frame
	void Update ()
    {
		++m_fps;
		m_Time += Time.deltaTime;
        if (m_Time > 1)
		{
			m_Time = 0;
			m_fpsToUI = m_fps;
			m_fps = 0;
        }
    }

    void OnGUI()
	{
		GUI.Label(new Rect(Screen.width /2, 0, 60, 60), m_fpsToUI.ToString(), m_fpsType);
    }
}
