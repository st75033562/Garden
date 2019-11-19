using UnityEngine.UI;

public class UIGradeNode : ScrollableCell
{
	public Text m_TaskName;
	public UITeacherGrade m_Leader;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        m_TaskName.text = taskInfo.m_Name;
	}

    public TaskInfo taskInfo
    {
        get { return (TaskInfo)DataObject; }
    }

	public void ClickNode()
	{
		m_Leader.SelectTask(taskInfo.m_ID);
	}
}
