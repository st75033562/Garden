using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIGradeTaskCellData
{
    public MemberInfo memberInfo;
    public TaskSubmitInfo submitInfo;

    private Project m_commentedProject;

    public Project commentedProject
    {
        get { return m_commentedProject; }
        set
        {
            m_commentedProject = value;
            if (m_commentedProject != null)
            {
                var messages = new LeaveMessageDataSource();
                messages.loadMessages(m_commentedProject.leaveMessageData);
                hasComments = messages.hasUserMessages(UserManager.Instance.UserId);
            }
            else
            {
                hasComments = false;
            }
        }
    }

    public bool hasComments
    {
        get;
        private set;
    }
}

public class UIGradeTaskCell : ScrollableCell
{
	public Text m_Nick;
	public Text m_Time;
	public GameObject m_UnCommitted;
	public GameObject m_ViewBtn;
	public GameObject m_GradeBtn;
	public GameObject m_CommentBtn;
	public UITeacherGradeTask m_Leader;
    public Text m_GradeText;

	public uint SubmitID
	{
		get { return data.submitInfo != null ? data.submitInfo.m_ID : 0; }
	}

    public UIGradeTaskCellData data
    {
        get { return (UIGradeTaskCellData)DataObject; }
    }

	public void ClickViewBtn()
	{
		m_Leader.ClickDownLoadStudentTask(SubmitID);
	}

	public void ClickGrade()
	{
		m_Leader.ShowGradeBtnList(SubmitID);
	}

	public void ClickComment()
	{
		m_Leader.LoadComment(SubmitID);
	}

    public override void ConfigureCellData()
    {
        var data = this.data;

        m_Nick.text = data.memberInfo.nickName;

        bool hasSubmit = data.submitInfo != null;

        m_UnCommitted.SetActive(!hasSubmit);
        m_Time.gameObject.SetActive(hasSubmit);
        m_ViewBtn.SetActive(hasSubmit);

        m_GradeBtn.SetActive(hasSubmit);
        if (hasSubmit)
        {
            m_Time.text = TimeUtils.GetLocalizedTime(data.submitInfo.m_Time);
            SetGrade(data.submitInfo.m_Grade);
        }
        else
        {
            SetGrade(-1);
        }

        m_CommentBtn.SetActive(data.submitInfo != null && data.hasComments);
    }

    private void SetGrade(int grade)
    {
        m_GradeText.gameObject.SetActive(grade > 0);
        
        if (grade > 0)
        {
            m_GradeText.text = GradeMark.GetString(grade).ToString();
        }
    }
}
