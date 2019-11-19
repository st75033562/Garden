using Robomation;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIRobotNode : MonoBehaviour {
    public RobotIcons m_RobotIcons;
    public Image m_RobotImage;
	public Text m_RobotName;
	public Text m_RobotMac;
	public Text m_RobotIndex;
    public GameObject m_ButtonCalibrate;

	Robot m_Robot;

	public void SetRobotData(Robot robot)
	{
        if (robot == null)
        {
            throw new ArgumentNullException("robot");
        }

		m_Robot = robot;
		m_RobotName.text = m_Robot.getName();
		m_RobotMac.text = m_Robot.uniqueId;
        m_ButtonCalibrate.SetActive(robot is HamsterRobot);
        m_RobotImage.sprite = m_RobotIcons.icons[(int)robot.type];
	}

	public void SetRobotIndex(int index)
	{
		m_RobotIndex.text = "ui_robot_number".Localize(index.ToString());
	}

	public Robot GetRobotData()
	{
		return m_Robot;
	}

	public void UpdateRobot()
	{
		m_RobotName.text = m_Robot.getName();
	}

    public string RotbotName() {
        return m_Robot.getName();
    }
}
