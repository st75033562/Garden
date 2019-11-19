using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Google.Protobuf;
using System.Collections.Generic;

using g_WebRequestManager = Singleton<WebRequestManager>;
using System.IO;
using System;

public class TaskCell : ScrollableCell
{
	enum MarkType
	{
		NEW,
		SUBMIT,
		FEEDBACK
	}

    public class AttachIdPath {
        public string path;
        public uint Id;
    }

	[SerializeField]
	private Text taskName;

    public Text gradeText;
    public GameObject passGo;

    public TaskInfoCellData infoData;

	public override void ConfigureCellData()
	{
		infoData = (TaskInfoCellData)dataObject;
		if (infoData == null || UserManager.Instance.CurClass == null)
			return;

		taskName.text = infoData.m_Name;

        var submitInfo = infoData.SubmitList.Find((x) => { return x.m_ID == UserManager.Instance.UserId; });
		if (submitInfo != null)
		{
            if(submitInfo.m_Grade != 0) {   //0默认值没有打分
                showMark(MarkType.FEEDBACK , submitInfo.m_Grade);
            } else {
                showMark(MarkType.SUBMIT);
            }
		}
		else
		{
			showMark(MarkType.NEW);
		}

        if (Preference.scriptLanguage == ScriptLanguage.Visual)
        {
            var commentRequest = Downloads.DownloadComment(
                UserManager.Instance.CurClass.m_ID,
                infoData.m_ID, 
                UserManager.Instance.UserId);
            commentRequest.userData = infoData;
            commentRequest.defaultErrorHandling = false;
            commentRequest.Success(files => {
                var curInfoData = (TaskInfoCellData)commentRequest.userData;
                curInfoData.m_CommentCode = files.GetFileData(CodeProjectRepository.ProjectFileName);
                curInfoData.m_CommentMessage = files.GetFileData(CodeProjectRepository.LeaveMessageFileName);
            })
            .Execute();
        }
	}

	public void OnClick()
	{
        infoData.taskSceneController.ShowTaskDetail(infoData, this);
	}

	public void OnClickDown(uint projectId, string name, AttachData.Type attachType, List<TaskEnclosureCell> cells)
	{
        if(attachType == AttachData.Type.Project) {
            AttachInfo attachInfo = new AttachInfo();
            attachInfo.resType = AttachInfo.ResType.Program;
            AttachProgram attachProgram = new AttachProgram();
            attachProgram.from = AttachProgram.From.Server;
            attachProgram.programPath = TaskCommon.GetTaskPath(UserManager.Instance.CurClass.m_ID, infoData.m_ID,
                UserManager.Instance.CurClass.teacherId) + "/" + projectId;
            attachInfo.programData = attachProgram;
            AttachmentPreview.Preview(attachInfo);
        } else {
            AttachInfo attachInfo = new AttachInfo();
            attachInfo.resType = AttachInfo.ResType.GameBoard;
            AttachGb attachData = new AttachGb();
            var proCells = cells.FindAll(x => { return x != null && x.attachType == AttachData.Type.Project; });
            attachData.relations = new List<string>();
            foreach (var proCell in proCells)
            {
                attachData.relations.Add(TaskCommon.GetTaskPath(UserManager.Instance.CurClass.m_ID, infoData.m_ID,
                UserManager.Instance.CurClass.teacherId) + "/" + proCell.projectId);
            }
            attachData.from = AttachGb.From.Server;
            attachData.openSave = true;
            attachData.nickName = name;
            attachData.programPath = TaskCommon.GetTaskPath(UserManager.Instance.CurClass.m_ID, infoData.m_ID,
                UserManager.Instance.CurClass.teacherId) + "/" + projectId;
            attachData.webRelationPath = TaskCommon.GetTaskPath(UserManager.Instance.CurClass.m_ID, infoData.m_ID,
                UserManager.Instance.CurClass.teacherId) + "/";
            attachInfo.gbData = attachData;
            AttachmentPreview.Preview(attachInfo);
        }
    }

    public void RemoveSubmit(TaskSubmitInfo info) {
        infoData.SubmitList.Remove(info);
        showMark(MarkType.NEW);
    }

    K8_Attach_Type toK8AttachType(AttachData.Type type) {
        if(type == AttachData.Type.Gameboard) {
            return K8_Attach_Type.KatGameboard;
        } else if(type == AttachData.Type.Project) {
            return K8_Attach_Type.KatProjects;
        } else {
            return K8_Attach_Type.KatImage;
        }
    }

    public void UpdateModify(List<ClassProgramStuCell> cells , Action done, FileList fileList = null) {
        if(cells.Count == 0) {
            done();
            return;
        }
        var taskAttach = new CMD_Modify_Task_Attach_r_Parameters();
        taskAttach.ClassId = UserManager.Instance.CurClass.m_ID;
        taskAttach.TaskId = infoData.m_ID;
        foreach (var cell in cells)
        {
            K8_Attach_Unit unit = new K8_Attach_Unit();
            unit.AttachType = toK8AttachType(cell.payload.attachType);
            unit.AttachName = cell.payload.nickName;
            if(cell.payload.relation) {
                unit.ClientData = cell.payload.nickName;
            } else {
                unit.ClientData = "";
            }
            if(fileList != null) {
                unit.AttachFiles = fileList;
            }
            taskAttach.ModifyInfo.Add(cell.payload.Id, unit);
        }
       
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdModifyTaskAttachR, taskAttach.ToByteString(), (res, data) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                TaskSubmitInfo taskSubInfo = infoData.SubmitList.Find((x) => { return x.m_ID == UserManager.Instance.UserId; });
                if(taskSubInfo != null && taskSubInfo.attachUnits != null) {
                    var modifyTaskA = CMD_Modify_Task_Attach_a_Parameters.Parser.ParseFrom(data);
                    foreach (var key in modifyTaskA.ModifyInfo.Keys)
                    { 
                        var csAu = taskSubInfo.attachUnits.Find(x => { return x.id == key; });
                        if(csAu != null) {
                            taskSubInfo.attachUnits.Remove(csAu);
                            AttachUnit au = new AttachUnit();
                            au.SetValue(modifyTaskA.ModifyInfo[key]);
                            ClassStuAttach classStuAttach = new ClassStuAttach();
                            classStuAttach.SetValue(key, au);

                            taskSubInfo.attachUnits.Add(classStuAttach);
                        }
                    }
                }
                done();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void Submit(List<ClassProgramStuCell> cells, bool submited, Action done) {
        if(cells.Count == 0) {
            done();
            return;
        }
        if(submited) {
            AddTaskAttach(cells, done);
        } else {
            CreateTask(cells, done);
        }
    }
    List<K8_Attach_Unit> PackAttach(List<ClassProgramStuCell> cells) {
        var attachUnits = new List<K8_Attach_Unit>();
        foreach (var idPath in cells)
        {
            var attachUnit = new K8_Attach_Unit();
            attachUnit.AttachName = Path.GetFileName(idPath.payload.nickName);
            attachUnit.AttachFiles = new FileList();

            if(idPath.payload.attachType == AttachData.Type.Gameboard) {
                attachUnit.AttachType = K8_Attach_Type.KatGameboard;
                var gbProject = GameboardRepository.instance.loadGameboardProject(idPath.payload.localPath);
                attachUnit.AttachFiles.FileList_.AddRange(gbProject.ToFileNodeList(""));
            } else if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                attachUnit.AttachType = K8_Attach_Type.KatProjects;
                var project = CodeProjectRepository.instance.loadCodeProject(idPath.payload.localPath);
                attachUnit.AttachFiles.FileList_.AddRange(project.ToFileNodeList(""));
                if(idPath.payload.relation) {
                    attachUnit.ClientData = idPath.payload.nickName;
                } else {
                    attachUnit.ClientData = "";
                }
            } else {
                attachUnit.AttachType = K8_Attach_Type.KatProjects;
                var project = PythonRepository.instance.loadProjectFiles(idPath.payload.localPath);
                attachUnit.AttachFiles.FileList_.AddRange(project.ToFileNodeList(""));
                if(idPath.payload.relation) {
                    attachUnit.ClientData = idPath.payload.nickName;
                } else {
                    attachUnit.ClientData = "";
                }
            }
            attachUnits.Add(attachUnit);
        }
        return attachUnits;
    }

    private void AddTaskAttach(List<ClassProgramStuCell> cells, Action done) {
        var taskAttach = new CMD_Add_Task_Attach_r_Parameters();
        taskAttach.ClassId = UserManager.Instance.CurClass.m_ID;
        taskAttach.TaskId = infoData.m_ID;
        var k8Attach = PackAttach(cells);
        taskAttach.AddInfo.AddRange(k8Attach);

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdAddTaskAttachR, taskAttach.ToByteString(), (res, data) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var attachA = CMD_Add_Task_Attach_a_Parameters.Parser.ParseFrom(data);
                List<ClassStuAttach> classStuAttachs = new List<ClassStuAttach>();
                foreach (uint key in attachA.AddInfo.Keys)
                {
                    AttachUnit attachUnit = new AttachUnit();
                    attachUnit.SetValue(attachA.AddInfo[key]);
                    ClassStuAttach classStuAttach = new ClassStuAttach();
                    classStuAttach.SetValue(key, attachUnit);
                    classStuAttachs.Add(classStuAttach);
                }
                TaskSubmitInfo taskSubInfo = infoData.SubmitList.Find((x) => { return x.m_ID == UserManager.Instance.UserId; });
                taskSubInfo.attachUnits.AddRange(classStuAttachs);
                done();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    private void CreateTask(List<ClassProgramStuCell> cells, Action done)
    {
        var submitTask = new A8_Student_Submit_Task();
        submitTask.SubmitId = UserManager.Instance.UserId;
        uint i = 1;
        submitTask.SubmitAttachInfo = new K8_Attach_Info();
        foreach (var attach in PackAttach(cells))
        {
            submitTask.SubmitAttachInfo.AttachList.Add(i++, attach);
        }

        var tSubmit = new CMD_Submit_Task_r_Parameters();
        tSubmit.ClassId = UserManager.Instance.CurClass.m_ID;
        tSubmit.TaskId = infoData.m_ID;
        tSubmit.SubmitTask = submitTask;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdSubmitTaskR, tSubmit.ToByteString(), (res, data) => {
            PopupManager.Close(popupId);

            if (res == Command_Result.CmdNoError)
            {
                var taskSubmitA = new TaskSubmitInfo();
                taskSubmitA.SetValue((CMD_Submit_Task_a_Parameters.Parser.ParseFrom(data)).SubmitTask);
                TaskSubmitInfo taskSubInfo = infoData.SubmitList.Find((x) => { return x.m_ID == UserManager.Instance.UserId; });
                if(taskSubInfo != null) {
                    taskSubInfo = taskSubmitA;
                } else {
                    infoData.SubmitList.Add(taskSubmitA);
                }
                showMark(MarkType.SUBMIT);
                done();
            }
            else
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }

	public void OnClickOpen()
	{
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            PopupManager.ProjectView((path) => {
                SceneDirector.Push("Main", CodeSceneArgs.FromPath(path.ToString()));
            });
        } else {
            PopupManager.PythonProjectView();
        }
       
	}

	void showMark(MarkType type , int grade = 0)
	{
        switch(type) {
            case MarkType.NEW:
                gradeText.transform.parent.gameObject.SetActive(false);
                passGo.SetActive(false);
                break;
            case MarkType.SUBMIT:
                gradeText.transform.parent.gameObject.SetActive(false);
                passGo.SetActive(true);
                break;
            case MarkType.FEEDBACK:
                gradeText.transform.parent.gameObject.SetActive(true);
                passGo.SetActive(false);
                gradeText.text = "class_grade_format".Localize(GradeMark.GetString(grade));
                break;
        }
	}

	public void OnClickComment()
	{
        Project project = new Project();
        project.code = infoData.m_CommentCode;
        project.leaveMessageData = infoData.m_CommentMessage;
        PopupManager.Workspace(CodeSceneArgs.FromTempCode(project));
    }
}
