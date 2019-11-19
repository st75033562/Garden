using UnityEngine;
using UnityEngine.UI;

public class UIMsgData : MonoBehaviour {
	public Text m_VarName;
    public GameObject m_DeleteButton;

    public void SetGlobal(bool global)
    {
        m_VarName.color = global ? Color.red : Color.black;
    }

    public void SetDeletable(bool deletable)
    {
        m_DeleteButton.SetActive(deletable);
    }
}
