using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIIconNode : MonoBehaviour
{
	[SerializeField]
	Image m_Icon;

	[SerializeField]
	GameObject m_SelectFrontImage;

	[SerializeField]
	int m_ID;

	// Use this for initialization
	void Start()
	{

	}

	public Image Icon
	{
		get
		{
			return m_Icon;
		}

		set
		{
			m_Icon = value;
		}
	}

	public int ID
	{
		get
		{
			return m_ID;
		}
	}

	public void SelectState(bool show)
	{
		m_SelectFrontImage.SetActive(show);
	}
}
