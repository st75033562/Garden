using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIMonitorRobot : MonoBehaviour
{
    public RobotIcons m_RobotIcons;
    public Image m_RobotImage;
	public Text m_RobotName;
    public float m_Interval;

    protected IRobot m_Robot;

    void OnEnable()
    {
        StartCoroutine(UpdateUI());
    }

    IEnumerator UpdateUI()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(Mathf.Max(m_Interval, 0));
            UpdateReadings();
        }
    }

    void UpdateReadings()
    {
        if (null != m_Robot)
        {
            DoUpdateReadings();
        }
    }

    protected abstract void DoUpdateReadings();

	public void SetRobot(IRobot robot)
	{
        if (robot == null)
        {
            throw new ArgumentNullException("robot");
        }

		m_Robot = robot;
        m_RobotName.text = robot.getName();
        m_RobotImage.sprite = m_RobotIcons.icons[(int)robot.type];
        UpdateReadings();
	}
}
