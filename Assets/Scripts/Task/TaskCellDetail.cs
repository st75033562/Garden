using Gameboard;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskEnclosureCell {
    public AttachData.Type attachType;
    public LocalResData localResData;
    public string programName;
    public string teacherId;
    public uint projectId;
    public TaskEnclosureCell(LocalResData localResData, string programName, uint id, AttachData.Type attachType) {
        this.localResData = localResData;
        this.programName = programName;
        projectId = id;
        this.attachType = attachType;
    }
}
public class TaskCellDetail : MonoBehaviour {
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text description;
	[SerializeField]
	private Button m_BtnComment;
    public Button btnSubmit;

    public GameObject attachCell;
    public Transform attachParent;
    public GameObject answerCell;
    public Transform answerParent;
    public RectTransform addAnswerRect;
    public GameObject[] prohibitSubmitGos;

    private TaskCell taskCell;
    private TaskInfoCellData taskInfo;
    
    private bool submited;
    private TaskSubmitInfo taskSubInfo { get { return taskInfo.SubmitList.Find((x) => { return x.m_ID == UserManager.Instance.UserId; }); } }
    private List<ClassProgramStuCell> answerCells = new List<ClassProgramStuCell>();
    private List<TaskEnclosureCell> resources;
    private List<AttachData> delAttachData = new List<AttachData>();
    public void InitData(TaskInfoCellData data, TaskCell taskCell)
    {
        taskInfo = data;
        this.taskCell = taskCell;
        title.text = data.m_Name;
        

        description.text = data.m_Description;

        foreach (GameObject go in prohibitSubmitGos)
        {
            go.SetActive(!data.prohibitSubmit);
        }
        answerCells.Clear();
        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            m_BtnComment.gameObject.SetActive(false);
        }
        m_BtnComment.interactable = data.m_CommentCode != null;
		UserManager.Instance.CurTask = data;
		UserManager.Instance.CurSubmit = taskSubInfo;
        resources = new List<TaskEnclosureCell>();
        if(data.attachs != null) {
            foreach(uint key in data.attachs.AttachList.Keys) {
                K8_Attach_Unit attach = data.attachs.AttachList[key];
                if(attach.AttachType == K8_Attach_Type.KatVideo) {
                    resources.Add(new TaskEnclosureCell(new LocalResData(attach.AttachUrl, attach.AttachName, ResType.Video), null, key, AttachData.Type.Res));
                } else if(attach.AttachType == K8_Attach_Type.KatImage) {
                    resources.Add(new TaskEnclosureCell(new LocalResData(attach.AttachUrl, attach.AttachName, ResType.Image), null, key, AttachData.Type.Res));
                } else if(attach.AttachType == K8_Attach_Type.KatCourse) {
                    resources.Add(new TaskEnclosureCell(new LocalResData(attach.AttachUrl, attach.AttachName, ResType.Course), null, key, AttachData.Type.Res));
                } else if(attach.AttachType == K8_Attach_Type.KatProjects) {
                    resources.Add(new TaskEnclosureCell(null, attach.AttachName, key, AttachData.Type.Project));
                } else if(attach.AttachType == K8_Attach_Type.KatGameboard) {
                    resources.Add(new TaskEnclosureCell(null, attach.AttachName, key, AttachData.Type.Gameboard));
                }
            }
        }

        attachParent.DestroyChildren();

        foreach(TaskEnclosureCell cell in resources)
        {
            GameObject go = Instantiate(attachCell, attachParent);
            go.SetActive(true);
            go.GetComponent<AttachmentCellWithTypeText>().SetData(cell);
        }

        btnSubmit.interactable = false;

        ToAnswers();
    }

    void ToAnswers() {
        if(taskSubInfo != null) {
            submited = true;
            if(taskSubInfo.attachUnits != null) {
                foreach(var attachUnit in taskSubInfo.attachUnits) {
                    AttachData.Type attachType;
                    if(attachUnit.attachUnit.attachType == K8_Attach_Type.KatGameboard) {
                        attachType = AttachData.Type.Gameboard;
                    } else if(attachUnit.attachUnit.attachType == K8_Attach_Type.KatProjects) {
                        attachType = AttachData.Type.Project;
                    } else {
                        attachType = AttachData.Type.Res;
                    }
                    RefreshAnswers(attachUnit.attachUnit.url, null, attachUnit.id, attachType, attachUnit.attachUnit.attachName,
                        AttachData.State.Initial, relation:!string.IsNullOrEmpty(attachUnit.attachUnit.clientData));
                }
            }
        } else {
            submited = false;
            RefreshAnswers(null, null, 0, AttachData.Type.Res, null, AttachData.State.Initial);
        }
    }

    public void OnClickDown(uint projectId, string name, AttachData.Type attachType)
    {
        taskCell.OnClickDown (projectId, name, attachType, resources);
    }

    public void OnClickUpload ()
    {
        List<AttachData> attachDatas = new List<AttachData>();
        foreach(ClassProgramStuCell cell in answerCells) {
            AttachData attachData = new AttachData();
            attachData.type = cell.payload.attachType;
            if(cell.payload.isLocal) {
                attachData.programPath = cell.payload.localPath;
            } else {
                attachData.webProgramPath = cell.payload.cellpath;
            }
            attachData.isRelation = cell.payload.relation;
            attachData.id = cell.payload.Id;
            attachData.state = cell.payload.state;
            attachData.programNickName = cell.payload.nickName;
            attachDatas.Add(attachData);
        }
       
        PopupManager.AttachmentManager(attachDatas, (upload) => {
            upload();
        }, () => {
            foreach(ClassProgramStuCell cell in answerCells) {
                Destroy(cell.gameObject);
            }
            answerCells.Clear();

            delAttachData.Clear();
            foreach(var attach in attachDatas) {
                if(attach.state == AttachData.State.Delete) {
                    delAttachData.Add(attach);
                    continue;
                }
                RefreshAnswers(attach.webProgramPath, attach.programPath, attach.id, attach.type,
                    attach.programNickName, attach.state, !string.IsNullOrEmpty(attach.programPath), relation:attach.isRelation);
            }
            btnSubmit.interactable = true;
        }, gameboardCount: PopupAttachmentManager.MaxAttachCount, showResouce:false, maxAttachCount:6);

    }

    public void OnClickSubmit() {
        OnClickDelete(delAttachData, ()=> {
            answerCells.RemoveAll(x => { return x.payload.state == AttachData.State.Delete; });
            var modifyCells = answerCells.FindAll(x => { return x.payload.state == AttachData.State.Initial; });
            taskCell.UpdateModify(modifyCells, () => {
                var newCells = answerCells.FindAll(x => { return x.payload.state == AttachData.State.NewAdd; });
                taskCell.Submit(newCells, submited, () => {
                    for(int i=0; i< answerCells.Count; i++) {
                        Destroy(answerCells[i].gameObject);
                    }
                    answerCells.Clear();
                    ToAnswers();
                    var gameboardAns = answerCells.FindAll(x => { return x.payload.state != AttachData.State.Delete && x.payload.attachType != AttachData.Type.Project; });

                    InstanceGb(gameboardAns, () => {
                        foreach(ClassProgramStuCell cell in answerCells) {
                            cell.payload.isLocal = false;
                        }
                        OnClickClose();
                        PopupManager.Notice("ui_submit_sucess".Localize());
                    });
                });
            });
        });
    }

    void InstanceGb(List<ClassProgramStuCell> gameboardAns, Action done) {
        if(gameboardAns.Count == 0) {
            done();
            return;
        } else if(gameboardAns.Count > 1) {
            ClearAllGbGroups(0, gameboardAns, done);
            return;
        }

        LoadGb(gameboardAns[0], (gb)=> {
            UploadChangedItem(gameboardAns[0], gb, done);
        });

        //if(gameboardAns[0].payload.isLocal) {
        //    Gameboard.Gameboard gb = GameboardRepository.instance.getGameboard(gameboardAns[0].payload.localPath);
        //    UploadChangedItem(gameboardAns[0], gb, done);
        //} else {
        //    ProjectDownloadRequestV3 m_download = new ProjectDownloadRequestV3();
        //    m_download.basePath = gameboardAns[0].payload.cellpath;
        //    m_download
        //        .Success(dir => {
        //            var project = dir.ToGameboardProject().gameboard;
        //            UploadChangedItem(gameboardAns[0], project, done);
        //        })
        //        .Execute();
        //}
    }

    void LoadGb(ClassProgramStuCell cell, Action<Gameboard.Gameboard> done) {
        if(cell.payload.isLocal) {
            Gameboard.Gameboard gb = GameboardRepository.instance.getGameboard(cell.payload.localPath);
            done(gb);
        } else {
            ProjectDownloadRequestV3 m_download = new ProjectDownloadRequestV3();
            m_download.basePath = cell.payload.cellpath;
            m_download
                .Success(dir => {
                    var project = dir.ToGameboardProject().gameboard;
                    done(project);
                })
                .Execute();
        }
    }

    void ClearAllGbGroups(int index, List<ClassProgramStuCell> gameboardAns, Action done) {
        if(index < gameboardAns.Count) {
            LoadGb(gameboardAns[index], (gb) => {
                gb.ClearCodeGroups();
                UpdateModify(gameboardAns[index], gb, ()=> {
                    ClearAllGbGroups(++index, gameboardAns, done);
                });
            });
        } else {
            done();
        }
    }

    void UploadChangedItem(ClassProgramStuCell gameboardAns, Gameboard.Gameboard gb, Action done) {
        gb.ClearCodeGroups();
        var groups = gb.GetCodeGroups(Preference.scriptLanguage);
        int i = 0;
        foreach(var attachUnit in taskSubInfo.attachUnits) {
            if(!string.IsNullOrEmpty(attachUnit.attachUnit.clientData)) {
                var remotePath = Gameboard.ProjectUrl.ToRemote(attachUnit.id.ToString());
                var group = new Gameboard.RobotCodeGroupInfo(remotePath);
                group.Add(i++);
                group.projectName = attachUnit.attachUnit.attachName;
                groups.Add(group);
            }
        }
        UpdateModify(gameboardAns, gb, done);
    }

    void UpdateModify(ClassProgramStuCell gameboardAns, Gameboard.Gameboard gb, Action done) {
        List<ClassProgramStuCell> list = new List<ClassProgramStuCell>();
        list.Add(gameboardAns);
        FileNode tGb = new FileNode();
        tGb.PathName = GameboardRepository.GameBoardFileName;
        tGb.FileContents = gb.Serialize().ToByteString();
        FileList fileList = new FileList();
        fileList.FileList_.Add(tGb);
        taskCell.UpdateModify(list, () => {
            done();
        }, fileList);
    }

    public void OnClickOpen ()
    {
        taskCell.OnClickOpen ();
    }

    public void OnClickClose ()
	{
		UserManager.Instance.CurTask = null;
		UserManager.Instance.CurSubmit = null;
        gameObject.SetActive (false);
        foreach (var cell in answerCells)
        {
            Destroy(cell.gameObject);
        }
    }

	public void OnClickComment()
	{
		taskCell.OnClickComment();
	}


    public void OnClickDelete(List<AttachData> cells, Action done) {
        if(cells.Count == 0) {
            done();
            return;
        }
        AttachData cell = cells[0];
        cells.Remove(cell);
        if(cell.id != 0) {
            int popupId = PopupManager.ShowMask();
            var del = new CMD_Del_Task_Attach_r_Parameters();
            del.ClassId = UserManager.Instance.CurClass.m_ID;
            del.TaskId = taskInfo.m_ID;
            del.AttachIds.Add(cell.id);

            SocketManager.instance.send(Command_ID.CmdDelTaskAttachR, del.ToByteString(), (res, data) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    if(taskSubInfo != null) {
                        taskSubInfo.attachUnits.RemoveAll(x => { return x.id == cell.id; });
                        if(taskSubInfo.attachUnits.Count == 0) {
                            taskCell.RemoveSubmit(taskSubInfo);
                        }
                    }
                    OnClickDelete(cells, done);
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {
            OnClickDelete(cells, done);
        }
    }

    public void OnClickPreview(ClassProgramStuCell cell) {
        if(cell.payload.isLocal) {
            if(cell.payload.attachType == AttachData.Type.Gameboard) {
                AttachInfo attachInfo = new AttachInfo();
                attachInfo.resType = AttachInfo.ResType.GameBoard;
                AttachGb attachData = new AttachGb();

                attachData.realRelations = new List<string>();
                foreach (var answerCell in answerCells)
                {
                    if(answerCell.payload.relation) {
                        if(!string.IsNullOrEmpty(answerCell.payload.localPath)) {
                            attachData.realRelations.Add(answerCell.payload.localPath);
                        } else {
                            attachData.realRelations.Add(ProjectUrl.ToRemote(answerCell.payload.cellpath));
                        }
                    }
                }
                attachData.from = AttachGb.From.Local;
                attachData.programPath = cell.payload.localPath;
                attachInfo.gbData = attachData;
                AttachmentPreview.Preview(attachInfo);
            } else if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                var project = CodeProjectRepository.instance.loadCodeProject(cell.payload.localPath);
                PopupManager.Workspace(CodeSceneArgs.FromTempCode(project));
            } else {
                PythonPreview.Preview(cell.payload.localPath);
            }
        } else {
            if(cell.payload.attachType == AttachData.Type.Gameboard) {
                AttachInfo attachInfo = new AttachInfo();
                attachInfo.resType = AttachInfo.ResType.GameBoard;
                AttachGb attachData = new AttachGb();
                if(Preference.scriptLanguage == ScriptLanguage.Python) {
                    attachData.relations = new List<string>();
                    foreach(var data in answerCells) {
                        if(data != null && data.payload.attachType == AttachData.Type.Project && !string.IsNullOrEmpty(data.payload.cellpath)) {
                            attachData.relations.Add(data.payload.cellpath);
                        }
                    }
                }
                attachData.realRelations = new List<string>();
                foreach(var answerCell in answerCells) {
                    if(answerCell.payload.relation) {
                        if(!string.IsNullOrEmpty(answerCell.payload.localPath)) {
                            attachData.realRelations.Add(answerCell.payload.localPath);
                        } else {
                            attachData.realRelations.Add(ProjectUrl.ToRemote(answerCell.payload.cellpath));
                        }
                    }
                }
                attachData.from = AttachGb.From.Server;
                attachData.programPath = cell.payload.cellpath;
                attachData.webRelationPath = cell.payload.cellpath.Substring(0, cell.payload.cellpath.LastIndexOf("/")) + "/";
                attachInfo.gbData = attachData;
                AttachmentPreview.Preview(attachInfo);

            } else {
                if(string.IsNullOrEmpty(cell.payload.cellpath)) {
                    Debug.LogError("download path is empty");
                    return;
                }
                var request = new ProjectDownloadRequest();
                request.basePath = cell.payload.cellpath;
                request.blocking = true;
                request.Success(tRt => {
                    if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                        var project = tRt.ToProject();
                        if(project != null) {
                            PopupManager.Workspace(CodeSceneArgs.FromTempCode(project));
                        } else {
                            Debug.LogError("project didn't download data");
                        }
                    } else {
                        PythonPreview.Preview(tRt);
                    }
                })
                .Execute();
            }
        }
    }

    void RefreshAnswers(string path, string idPath, uint id, AttachData.Type type, string nickName, AttachData.State state,
        bool isLocal = false, bool showDelete = false, bool relation = false) {
        ClassProgramStuCell.PayLoad programDatas = new ClassProgramStuCell.PayLoad();
        if(path != null || idPath != null) {
            programDatas.localPath = idPath;
            programDatas.Id = id;
            programDatas.attachType = type;
            programDatas.state = state;
            programDatas.nickName = nickName;
            programDatas.cellpath = path;
            programDatas.isLocal = isLocal;
            programDatas.showDelete = showDelete;
            programDatas.relation = relation;
            var classProgramStuCell = Instantiate(answerCell, answerParent).GetComponent<ClassProgramStuCell>();
            classProgramStuCell.gameObject.SetActive(true);
            classProgramStuCell.SetData(programDatas);
            answerCells.Add(classProgramStuCell);
        }
        if(taskSubInfo == null || taskSubInfo.m_Grade == 0) {
            addAnswerRect.gameObject.SetActive(true);
            addAnswerRect.SetAsLastSibling();
        } else {
            addAnswerRect.gameObject.SetActive(false);
        }
    }

    public void OnClickAddAnswer() {
        OnClickUpload();
    }
}
