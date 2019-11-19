using UnityEngine;

public class UIMonitor : MonoBehaviour
{
    public GameObject[] m_RobotMonitorTemplates;

    public RectTransform m_Content;
    public UIMonitorVariable m_VarMonitor;

    public void Init(IRobotManager robotManager, VariableManager varManager)
    {
        m_VarMonitor.Configure(varManager);
        m_VarMonitor.gameObject.SetActive(true);

        foreach (var robot in robotManager)
        {
            if ((int)robot.type < m_RobotMonitorTemplates.Length)
            {
                var instance = (GameObject)Instantiate(m_RobotMonitorTemplates[(int)robot.type], m_Content);
                instance.SetActive(true);
                UIMonitorRobot monitor = instance.GetComponent<UIMonitorRobot>();
                monitor.SetRobot(robot);
            }
            else
            {
                Debug.LogError("unrecognized robot type: " + robot.type);
            }
        }
    }
}
