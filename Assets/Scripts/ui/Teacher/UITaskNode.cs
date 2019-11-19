using UnityEngine;
using UnityEngine.UI;

public class UITaskNode : ScrollableCell
{
	public Text m_TaskName;
	public GameObject m_Mask;
	public UITeacherTask m_Task;

	public TaskInfo taskInfo
    {
        get { return (TaskInfo)dataObject; }
    }

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

		m_TaskName.text = taskInfo.m_Name;
        m_Mask.SetActive(m_Task.isDeleting);
    }

    public void ClickNode()
    {
        if (m_Mask.activeSelf)
        {
            m_Task.DeleteTask(taskInfo.m_ID);
        }
        else
        {
            m_Task.EditTask(taskInfo.m_ID);
        }
    }
}
