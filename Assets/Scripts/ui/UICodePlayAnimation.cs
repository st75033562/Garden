using UnityEngine;
using System.Collections;

public class UICodePlayAnimation : MonoBehaviour
{
	public GameObject[] m_Man;
	public GameObject m_Line;

	const float c_ManSpace = 0.1f;
	const float c_LineSpace = 0.3f;
	float m_CurPlayTime;
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		m_CurPlayTime += Time.unscaledDeltaTime;
		int tCurManIndex = (int)(m_CurPlayTime / c_ManSpace) % m_Man.Length;
		for(int i = 0; i < m_Man.Length; ++i)
		{
			m_Man[i].SetActive(i == tCurManIndex);
		}
		m_Line.SetActive(0 == tCurManIndex % 2);
	}

	void OnEnable()
	{
		m_CurPlayTime = 0;
	}
}
