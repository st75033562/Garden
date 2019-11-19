using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIVariableData : MonoBehaviour {
	public Image m_Icon;
	public Text m_VarName;
    public GameObject m_DeleteButtonObj;
    public Text m_ReservedText;

    public string varName
    {
        get { return m_VarName.text; }
        set { m_VarName.text = value; }
    }

    public void SetGlobal(bool global)
    {
        m_VarName.color = global ? Color.red : Color.black;
    }

    public void SetReserved(bool reserved)
    {
        m_DeleteButtonObj.SetActive(!reserved);
        m_ReservedText.enabled = reserved;
    }
}
