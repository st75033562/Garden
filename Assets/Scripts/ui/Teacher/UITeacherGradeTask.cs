using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class UITeacherGradeTask : MonoBehaviour {
    public Text m_title;
	public Text m_Count;
	public Text m_TimeText;
	public GameObject m_NodeTemplate;
	public GameObject m_GradeBtnList;
    public ScrollableAreaController m_ScrollController;

	enum Category
	{
		All,
		Uncommitted,
		Uncorrected,
	}

	Category m_CurrentCategory = Category.All;
    List<UIGradeTaskCellData> m_AllSubmits;

	uint m_OperationSubcomitID;

	// Use this for initialization
	void Start()
	{
        m_TimeText.text = "task_submit_date".Localize();
	}

	void OnEnable()
	{
        m_title.text = UserManager.Instance.CurTask.m_Name;

        PopulateSubmits();
        DownloadComments();

        m_CurrentCategory = Category.All;
      //  m_ScrollController.scrollPosition = 0.0f;
        Refresh();
	}

	void OnDisable()
	{
        m_AllSubmits = null;
	}

    void PopulateSubmits()
    {
        var students = UserManager.Instance.CurClass.studentsInfos;
        var submits = UserManager.Instance.CurTask.SubmitList;

        // perform a left outer join on students and submits
        m_AllSubmits = (from student in students
                       join submit in submits on student.userId equals submit.m_ID into gs
                       from s in gs.DefaultIfEmpty()
                       select new UIGradeTaskCellData {
                            memberInfo = student,
                            submitInfo = s
                       }).ToList();
    }

    void DownloadComments()
    {
        if (UserManager.Instance.CurClass.languageType != ScriptLanguage.Visual)
        {
            return;
        }

        foreach (var submit in m_AllSubmits.Where(x => x.submitInfo != null))
        {
            var request = Downloads.DownloadComment(
                UserManager.Instance.CurClass.m_ID,
                UserManager.Instance.CurTask.m_ID,
                submit.submitInfo.m_ID);

            request.userData = submit;
            request.blocking = true;
            request.defaultErrorHandling = false;
            request.Success(files => {
                    var curSubmit = (UIGradeTaskCellData)request.userData;
                    curSubmit.commentedProject = files.ToProject();
                    m_ScrollController.model.updatedItem(curSubmit);
                })
                .Execute();
        }
    }

	void Refresh()
	{
		switch(m_CurrentCategory)
		{
        case Category.All:
            ShowAll();
            break;
        case Category.Uncommitted:
            ShowUncommitted();
            break;
        case Category.Uncorrected:
            ShowUncorrected();
            break;
		}
    }

	void ShowAll()
	{
        int curSubmitNum = m_AllSubmits.Count(x => x.submitInfo != null);
		m_Count.text = "task_submission_rate".Localize(curSubmitNum, UserManager.Instance.CurClass.studentsInfos.Count);
        m_ScrollController.InitializeWithData(m_AllSubmits);
	}

	void ShowUncommitted()
	{
        var uncommited = m_AllSubmits.Where(x => x.submitInfo == null).ToArray();
		m_Count.text = "task_uncommited_rate".Localize(uncommited.Length, UserManager.Instance.CurClass.studentsInfos.Count);
        m_ScrollController.InitializeWithData(uncommited);
	}

	void ShowUncorrected()
	{
        var uncorrected = m_AllSubmits.Where(x => x.submitInfo != null && x.submitInfo.m_Grade == 0).ToArray();
		m_Count.text = "task_uncorrected_rate".Localize(uncorrected.Length, UserManager.Instance.CurClass.studentsInfos.Count);
        m_ScrollController.InitializeWithData(uncorrected);
	}

	public void ClickAll()
	{
		m_CurrentCategory = Category.All;
		Refresh();
	}

	public void ClickUnCommitted()
	{
		m_CurrentCategory = Category.Uncommitted;
		Refresh();
	}

	public void ClickUncritical()
	{
		m_CurrentCategory = Category.Uncorrected;
		Refresh();
	}

	public void ClickReturn()
	{
		UserManager.Instance.CurTask = null;
        gameObject.SetActive(false);
		m_GradeBtnList.SetActive(false);
	}

	public void ShowGradeBtnList(uint ID)
	{
		m_OperationSubcomitID = ID;
        m_GradeBtnList.SetActive(true);
    }

	public void ClickGradeBtn(int level)
	{
		CMD_Grade_Task_r_Parameter tGradeSubmit = new CMD_Grade_Task_r_Parameter();
		tGradeSubmit.ClassId = UserManager.Instance.CurClass.m_ID;
		tGradeSubmit.TaskId = UserManager.Instance.CurTask.m_ID;
		tGradeSubmit.SubmitId = m_OperationSubcomitID;
		tGradeSubmit.GradeString = level.ToString();
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGradeTaskR, tGradeSubmit.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var response = CMD_Grade_Task_a_Parameter.Parser.ParseFrom(content);
                var classInfo = UserManager.Instance.GetClass(response.ClassId);
                var task = classInfo.GetTask(response.TaskId);

                var submit = task.SubmitList.FirstOrDefault(x => x.m_ID == response.SubmitId);
                if(submit != null) {
                    submit.m_Grade = int.Parse(response.GradeString);
                    int index = m_AllSubmits.FindIndex(x => x.submitInfo == submit);
                    m_ScrollController.model.updatedItem(index);
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });

        m_GradeBtnList.SetActive(false);
		m_OperationSubcomitID = 0;
    }

	public void ClickDownLoadStudentTask(uint commitID)
	{
		TaskInfo tCurTask = UserManager.Instance.CurTask;
		TaskSubmitInfo tCurSub = null;
		for (int i = 0; i < tCurTask.SubmitList.Count; ++i)
		{
			tCurSub = tCurTask.SubmitList[i];
			if (tCurSub.m_ID == commitID)
			{
				break;
			}
		}

        

        if (null != tCurSub)
		{
            List<AddAttachmentCellData> attachmenetCells = new List<AddAttachmentCellData>();
            List<AttachData> attachDatas = new List<AttachData>();
            foreach (ClassStuAttach unit in tCurSub.attachUnits)
            {
                AttachData attachData = new AttachData();
                if(unit.attachUnit.attachType == K8_Attach_Type.KatGameboard) {
                    attachData.type = AttachData.Type.Gameboard;
                } else if(unit.attachUnit.attachType == K8_Attach_Type.KatProjects) {
                    attachData.type = AttachData.Type.Project;
                } else {
                    attachData.type = AttachData.Type.Res;
                }
                attachData.programNickName = unit.attachUnit.attachName;
                attachData.webProgramPath = TaskCommon.GetTaskPath(UserManager.Instance.CurClass.m_ID,
                    UserManager.Instance.CurTask.m_ID, commitID) + "/" + unit.id;
                attachDatas.Add(attachData);
                attachmenetCells.Add(new AddAttachmentCellData(attachData, attachDatas)) ;
            }

            PopupManager.ViewStuSubAtch(attachmenetCells);
        }
		else
		{
			Debug.LogError("ClickDownLoadStudentTask error, can't find member");
			return;
		}
		UserManager.Instance.CurSubmit = tCurSub;
    }

	public void CodeDownLoadFail(WebRequestData data)
	{
        PopupManager.YesNo("download_failed_try_again".Localize(), () => {
            DownloadAgin(data);
        }, () => {
            UserManager.Instance.CurSubmit = null;
        });
	}

	public void DownloadAgin(WebRequestData tWeb)
	{
        g_WebRequestManager.instance.AddTask(tWeb);
	}

	public void LoadComment(uint commitID)
	{
		TaskInfo tCurTask = UserManager.Instance.CurTask;
        var submitData = m_AllSubmits.Find(x => x.submitInfo != null && x.submitInfo.m_ID == commitID);
        if (submitData == null)
        {
			Debug.LogError("invalid commit id");
            return;
        }
		UserManager.Instance.CurSubmit = submitData.submitInfo;
        PopupManager.Workspace(CodeSceneArgs.FromTempCode(submitData.commentedProject));
	}
}
