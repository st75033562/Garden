using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Robomation;

public class UIRenameDialog : MonoBehaviour
{
	public Text m_title;
    public InputField m_InputText;
	public Button m_Confirm;

	UIRobotManager m_Manager;
	HamsterRobot m_CurRobot;

	public void InitByManagerInAwake(UIRobotManager manager)
	{
		m_Manager = manager;
		m_Confirm.interactable = false;
		m_InputText.text = "";
    }

	public void CheckRenameInput()
	{
        m_Confirm.interactable = m_InputText.text.Length != 0;
	}

	public void CloseDialog()
	{
		gameObject.SetActive(false);
		m_CurRobot = null;
	}

	public void OpenDialog(HamsterRobot robot)
	{
		gameObject.SetActive(true);
		m_CurRobot = robot;
		m_Confirm.interactable = false;
        m_InputText.text = robot.getName();
	}

	public void Confirm()
	{
		if(m_Manager)
		{
			m_Manager.EditComplete(m_CurRobot, m_InputText.text);
        }
		CloseDialog();
    }
}
