using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILobbyMask : MonoBehaviour {
	public Text m_Notice;
	// Use this for initialization
	void Start () {
	
	}
	
	public void SetActive(bool show)
	{
		gameObject.SetActive(show);
	}

	public void SetNotice(string txt)
	{
		m_Notice.text = txt;
	}
}
