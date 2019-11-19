using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBotIndex : MonoBehaviour {
	public Text m_Index;

    public int index
    {
        get { return int.Parse(m_Index.text); }
        set { m_Index.text = value.ToString(); }
    }
}
