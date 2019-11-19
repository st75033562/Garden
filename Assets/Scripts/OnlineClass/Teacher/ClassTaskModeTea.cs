using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class K8AttachAndAttachSwitch {
    static uint UUId(List<AttachData> attachDatas, uint key) {
        while(attachDatas.Find(x => { return x != null && x.id == key && x.state != AttachData.State.Delete; }) != null) {
            key++;
        }
        return key;
    }
    public static K8_Attach_Info ToK8Attach(List<AttachData> attachDatas, bool cleanBinding = false) {
        uint key = 1;
        var attachInfo = new K8_Attach_Info();
        foreach(AttachData res in attachDatas) {
            if(res == null || res.state == AttachData.State.Delete) {
                continue;
            }
            if(res.id == 0) {
                res.id = UUId(attachDatas, key);
            }

            if(res.resData != null) {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachUrl = res.resData.name;
                attachUnit.AttachName = res.resData.nickName;
                switch(res.resData.resType) {
                    case ResType.Video:
                        attachUnit.AttachType = K8_Attach_Type.KatVideo;
                        break;
                    case ResType.Image:
                        attachUnit.AttachType = K8_Attach_Type.KatImage;
                        break;
                    case ResType.Course:
                        attachUnit.AttachType = K8_Attach_Type.KatCourse;
                        break;
                }
                attachInfo.AttachList.Add(res.id, attachUnit);
            } else if(!string.IsNullOrEmpty(res.programPath)) {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = SwitchType(res);
                attachUnit.AttachFiles = new FileList();
                SetRelation(attachUnit, res);
                PackFileList(attachUnit, res.programPath, res.type, cleanBinding);
                attachInfo.AttachList.Add(res.id, attachUnit);
            } else if(res.webFileList != null) {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = SwitchType(res);
                attachUnit.AttachFiles = res.webFileList;
                SetRelation(attachUnit, res);
                attachInfo.AttachList.Add(res.id, attachUnit);
            } else {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = SwitchType(res);
                SetRelation(attachUnit, res);
                attachInfo.AttachList.Add(res.id, attachUnit);
            }
        }
        return attachInfo;
    }


    static void SetRelation(K8_Attach_Unit attachUnit, AttachData res) {
        if(res.isRelation) {
            attachUnit.ClientData = res.programNickName;
        } else {
            attachUnit.ClientData = "";
        }
    }

    public static K8_Attach_Info ToK8AttachOnlyInfo(List<AttachData> attachDatas) {
        var attachInfo = new K8_Attach_Info();
        foreach(AttachData res in attachDatas) {
            if(res == null || res.state == AttachData.State.Delete || res.state == AttachData.State.NewAdd) {
                continue;
            }
            if(res.resData != null) {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachUrl = res.resData.name;
                attachUnit.AttachName = res.resData.nickName;
                switch(res.resData.resType) {
                    case ResType.Video:
                        attachUnit.AttachType = K8_Attach_Type.KatVideo;
                        break;
                    case ResType.Image:
                        attachUnit.AttachType = K8_Attach_Type.KatImage;
                        break;
                    case ResType.Course:
                        attachUnit.AttachType = K8_Attach_Type.KatCourse;
                        break;
                }
                attachInfo.AttachList.Add(res.id, attachUnit);
            } else {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = SwitchType(res);
                SetRelation(attachUnit, res);
                attachInfo.AttachList.Add(res.id, attachUnit);
            }
        }
        return attachInfo;
    }

    public static void PackFileList(K8_Attach_Unit attachUnit, string programPath, AttachData.Type type, bool cleanBinding = false) {
        attachUnit.AttachFiles = new FileList();
        if(type == AttachData.Type.Project) {
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                var project = CodeProjectRepository.instance.loadCodeProject(programPath);
                attachUnit.AttachFiles.FileList_.AddRange(project.ToFileNodeList(""));
            } else {
                string dirPath = "";
                if(!string.IsNullOrEmpty(Path.GetDirectoryName(programPath))) {
                    dirPath = Path.GetDirectoryName(programPath) + "/";
                }

                var pythonFiles = PythonRepository.instance.loadProjectFiles(programPath);
                attachUnit.AttachFiles.FileList_.AddRange(pythonFiles.ToFileNodeList(dirPath));
            }
        } else if(type == AttachData.Type.Gameboard) {
            var gbProject = GameboardRepository.instance.loadGameboardProject(programPath);
            if(cleanBinding && gbProject.gameboard != null) {
                gbProject.gameboard.ClearCodeGroups();
            }
            attachUnit.AttachFiles.FileList_.AddRange(gbProject.ToFileNodeList(""));
        } 
    }

    public static K8_Attach_Type SwitchType(AttachData res) {
        if(res.type == AttachData.Type.Project) {
            return K8_Attach_Type.KatProjects;
        } else if(res.type == AttachData.Type.Gameboard) {
            return K8_Attach_Type.KatGameboard;
        } else {
            throw new Exception("Unrealized");
        }
    }


    public static List<AttachData> ToAttach(K8_Attach_Info attachs, string webPath) {
        List<AttachData> attachDatas = new List<AttachData>();
        foreach(uint key in attachs.AttachList.Keys) {
            K8_Attach_Unit attach = attachs.AttachList[key];
            attachDatas.Add(K8ToAttach(attachs.AttachList[key], key, webPath));
        }
        return attachDatas;
    }

    public static AttachData K8ToAttach(K8_Attach_Unit attach, uint id, string webPath) {
        if(attach.AttachType == K8_Attach_Type.KatProjects) {
            return new AttachData {
                type = AttachData.Type.Project,
                programNickName = attach.AttachName,
                webProgramPath = webPath + "/" + id,
                state = AttachData.State.Initial,
                id = id,
                webFileList = attach.AttachFiles,
                isRelation = !string.IsNullOrEmpty(attach.ClientData)
            };
        } else if(attach.AttachType == K8_Attach_Type.KatGameboard) {
            AttachData data = new AttachData {
                type = AttachData.Type.Gameboard,
                programNickName = attach.AttachName,
                webProgramPath = webPath + "/" + id,
                state = AttachData.State.Initial,
                id = id,
                webFileList = attach.AttachFiles
            };
            if(attach.AttachFiles != null) {
                data.gameboard = attach.AttachFiles.ToGameboardProject().gameboard;
            }
            return data;
        } else {
            var res = new LocalResData();
            res.name = attach.AttachUrl;
            res.nickName = attach.AttachName;
            if(attach.AttachType == K8_Attach_Type.KatVideo) {
                res.resType = ResType.Video;
            } else if(attach.AttachType == K8_Attach_Type.KatImage) {
                res.resType = ResType.Image;
            } else if(attach.AttachType == K8_Attach_Type.KatCourse) {
                res.resType = ResType.Course;
            }
            return new AttachData { resData = res, type = AttachData.Type.Res, state = AttachData.State.Initial, id = id };
        }
    }
}

public class ClassTaskModeTea {
    private UITeacherEditTask controller;
    public ClassTaskModeTea(UITeacherEditTask controller) {
        this.controller = controller;
    }

    void SerializeAttach(Action done) {
        List<AttachData> serializeRes = controller.attachDatas.FindAll(x=> {
            return x != null && x.state != AttachData.State.Delete && !string.IsNullOrEmpty(x.webProgramPath); });
        int finishCount = 0;
        if(serializeRes.Count > 0) {
            int popupId = PopupManager.ShowMask();
            foreach (var res in serializeRes)
            {
                var request = new ProjectDownloadRequest();
                request.basePath = res.webProgramPath;
                request.blocking = true;
                request.Success(tRt => {
                    res.webFileList = tRt;
                    if(++finishCount == serializeRes.Count) {
                        PopupManager.Close(popupId);
                        done();
                    }
                })
                .Execute();
            }
        }else{
            done();
        }
    }

    public void CreateTask(Action done) {
        if(controller.curClass.GetTask(controller.taskNameInputText) != null) {
            PopupManager.Notice("text_repeat_task_name".Localize());
            return;
        }
        SerializeAttach(()=> {
            var tCreate = new CMD_Create_Task_r_Parameters();
            tCreate.ClassId = controller.curClass.m_ID;
            tCreate.TaskInfo = new A8_Task_Info();
            tCreate.TaskInfo.TaskName = controller.taskNameInputText;
            tCreate.TaskInfo.TaskDescription = controller.descriptionInfo;
            tCreate.TaskInfo.TaskNotAllowSubmit = !controller.prohibitSubmitTog.isOn;
            tCreate.TaskInfo.TaskAttachInfoNew = K8AttachAndAttachSwitch.ToK8Attach(controller.attachDatas, true);

            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdCreateTaskR, tCreate.ToByteString(), (res, content) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    var response = CMD_Create_Task_a_Parameters.Parser.ParseFrom(content);
                    var tCurTask = controller.curClass.AddTask(response.TaskInfo);
                    ModifyGbUpload(response.TaskInfo.TaskAttachInfoNew, response.TaskInfo.TaskId, (key, gbAttach) => {
                        if(gbAttach != null && tCurTask.attachs.AttachList.ContainsKey(key)) {
                            tCurTask.attachs.AttachList[key] = gbAttach;
                        }
                        done();
                    });
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        });
    }

    public void EditConfirm(Action done) {
        var existingTask = controller.curClass.GetTask(controller.taskNameInputText);
        if(existingTask != null && existingTask.m_ID != controller.editorTaskId) {
            PopupManager.Notice("text_repeat_task_name".Localize());
            return;
        }
        CMD_Update_Task_r_Parameters tUpdate = new CMD_Update_Task_r_Parameters();
        tUpdate.ClassId = controller.curClass.m_ID;
        tUpdate.TaskInfo = new A8_Task_Info();
        tUpdate.TaskInfo.TaskId = controller.editorTaskId;
        tUpdate.TaskInfo.TaskName = controller.taskNameInputText;
        tUpdate.TaskInfo.TaskDescription = controller.descriptionInfo;
        tUpdate.TaskInfo.TaskNotAllowSubmit = !controller.prohibitSubmitTog.isOn;

        tUpdate.TaskInfo.TaskAttachInfoNew = K8AttachAndAttachSwitch.ToK8AttachOnlyInfo(controller.attachDatas);
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdUpdateTaskR, tUpdate.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Update_Task_a_Parameters tSuccess = CMD_Update_Task_a_Parameters.Parser.ParseFrom(content);
                TaskInfo tCurTask = controller.curClass.GetTask(tSuccess.TaskInfo.TaskId);
                tCurTask.SetValue(tSuccess.TaskInfo);
                UploadAddProgram((addTaskAttach) => {
                    if(addTaskAttach != null) {
                        foreach(uint key in addTaskAttach.AddInfo.Keys) {
                            controller.curTaskInfo.attachs.AttachList.Add(key, addTaskAttach.AddInfo[key]);
                        }
                    }
                    DeleteServiceAttach((delTaskAttach) => {
                        if(delTaskAttach != null) {
                            foreach(uint delId in delTaskAttach.AttachIds) {
                                controller.curTaskInfo.attachs.AttachList.Remove(delId);
                            }
                        }
                        ModifyGbUpload(tSuccess.TaskInfo.TaskAttachInfoNew, tSuccess.TaskInfo.TaskId,(key, gbAttach) => {
                            if(gbAttach != null && tCurTask.attachs.AttachList.ContainsKey(key)) {
                                tCurTask.attachs.AttachList[key] = gbAttach;
                            }
                            done();
                        });
                    });
                });

            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void UploadAddProgram(Action<CMD_Add_Task_Attach_a_Parameters> done) {  //task已经创建后，添加program
        var taskProgram = new CMD_Add_Task_Attach_r_Parameters();
        taskProgram.ClassId = UserManager.Instance.CurClass.m_ID;
        taskProgram.TaskId = controller.editorTaskId;
        foreach(AttachData res in controller.attachDatas) {
            if(res == null || res.state != AttachData.State.NewAdd) {
                continue;
            }
            if(res.resData != null) {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachUrl = res.resData.name;
                attachUnit.AttachName = res.resData.nickName;
                switch(res.resData.resType) {
                    case ResType.Video:
                        attachUnit.AttachType = K8_Attach_Type.KatVideo;
                        break;
                    case ResType.Image:
                        attachUnit.AttachType = K8_Attach_Type.KatImage;
                        break;
                    case ResType.Course:
                        attachUnit.AttachType = K8_Attach_Type.KatCourse;
                        break;
                }
                taskProgram.AddInfo.Add(attachUnit);
            } else {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = K8AttachAndAttachSwitch.SwitchType(res);
                K8AttachAndAttachSwitch.PackFileList(attachUnit, res.programPath, res.type, true);
                taskProgram.AddInfo.Add(attachUnit);
            }

        }

        if(taskProgram.AddInfo.Count > 0) {
            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdAddTaskAttachR, taskProgram.ToByteString(), (res, content) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    done(CMD_Add_Task_Attach_a_Parameters.Parser.ParseFrom(content));
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {
            done(null);
        }
    }

    void DeleteServiceAttach(Action<CMD_Del_Task_Attach_a_Parameters> done) {
        var delAttach = new CMD_Del_Task_Attach_r_Parameters();
        delAttach.ClassId = UserManager.Instance.CurClass.m_ID;
        delAttach.TaskId = controller.editorTaskId; ;
        foreach(AttachData res in controller.attachDatas) {
            if(res == null || res.state != AttachData.State.Delete) {
                continue;
            }
            delAttach.AttachIds.Add(res.id);
        }
        if(delAttach.AttachIds.Count > 0) {
            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelTaskAttachR, delAttach.ToByteString(), (res, content) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    done(CMD_Del_Task_Attach_a_Parameters.Parser.ParseFrom(content));
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {
            done(null);
        }
    }

    private bool CheckDuplicatePoolTask(TaskTemplate poolTaskCellData) {
        Assert.IsNotNull(poolTaskCellData);

        List<TaskTemplate> tasks = null;
        if(poolTaskCellData.type == TaskTemplateType.User) {
            tasks = NetManager.instance.taskPools;
        } else {
            tasks = NetManager.instance.sysTaskPools;
        }
        if(tasks.Any(x => x.name == controller.taskNameInputText && x.id != poolTaskCellData.id)) {
            PopupManager.Notice("repat_name_need_change".Localize());
            return true;
        }
        return false;
    }

    void ModifyGbUpload(K8_Attach_Info attachInfo, uint taskId, Action<uint, K8_Attach_Unit> done) {
        var attachList = attachInfo.AttachList.Values.ToList();
        var gbAttachUnit = attachList.Find(x=> { return x.AttachType == K8_Attach_Type.KatGameboard; }) ;
        if(gbAttachUnit == null) {
            done(0, null);
            return;
        }
        Gameboard.Gameboard gb = gbAttachUnit.AttachFiles.GetGameboard();
        uint gbKey = 0;
        if(gbAttachUnit != null) {
            gb.ClearCodeGroups();
            RobotCodeGroups groups = gb.GetCodeGroups(Preference.scriptLanguage);
            int i = 0;
            foreach (uint key in attachInfo.AttachList.Keys)
            {
                if(attachInfo.AttachList[key] == gbAttachUnit) {
                    gbKey = key;
                }
                if(attachInfo.AttachList[key].AttachType == K8_Attach_Type.KatProjects && !string.IsNullOrEmpty(attachInfo.AttachList[key].ClientData)) {
                    var remotePath = Gameboard.ProjectUrl.ToRemote(key.ToString());
                    var group = new Gameboard.RobotCodeGroupInfo(remotePath);
                    group.Add(i++);
                    group.projectName = attachInfo.AttachList[key].AttachName;
                    groups.Add(group);
                }
            }

            int maskId = PopupManager.ShowMask();
            var updateGb = new CMD_Modify_Task_Attach_r_Parameters();
            updateGb.ClassId = controller.curClass.m_ID;
            updateGb.TaskId = taskId;

            var attachUnit = new K8_Attach_Unit();

            attachUnit.AttachType = K8_Attach_Type.KatGameboard;
            attachUnit.AttachName = gbAttachUnit.AttachName;
            FileNode tGb = new FileNode();
            tGb.PathName = GameboardRepository.GameBoardFileName;
            tGb.FileContents = gb.Serialize().ToByteString();
            attachUnit.AttachFiles = new FileList();
            attachUnit.AttachFiles.FileList_.Add(tGb);

            updateGb.ModifyInfo.Add(gbKey, attachUnit);

            SocketManager.instance.send(Command_ID.CmdModifyTaskAttachR, updateGb.ToByteString(), (res, data) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    FileNode node = gbAttachUnit.AttachFiles.GetFile(GameboardRepository.GameBoardFileName);
                    if(node != null) {
                        node.FileContents = tGb.FileContents;
                    }
                    if(done != null) {
                        done(gbKey, gbAttachUnit);
                    }
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });

        } else {
            done(0, null);
        }
    }

    void InstanceProjectGb(List<AttachData> datas, Action done) {
        if(datas.Count == 0) {
            done();
            return;
        }
        var data = datas[0];
        datas.Remove(data);

        var m_currentTask = new ProjectDownloadRequestV3();
        m_currentTask.basePath = data.webProgramPath;
        m_currentTask.preview = true;
        m_currentTask.defaultErrorHandling = false;
        m_currentTask
            .Success(dir => {
                data.webFileList = dir;
                InstanceProjectGb(datas, done);
            })
            .Execute();
    }
    public void UploadPoolTask(TaskTemplate poolTaskCellData, Action done) {
        if(CheckDuplicatePoolTask(poolTaskCellData)) {
            return;
        }

        CheckDelProject(() => {
            var ProOrGb = controller.attachDatas.FindAll(x=> { return x != null && (x.type == AttachData.Type.Gameboard || x.type == AttachData.Type.Project)
                 && !string.IsNullOrEmpty(x.webProgramPath) && x.webFileList == null; });
            InstanceProjectGb(ProOrGb, ()=> {
                var request = new UploadTemplateTaskRequest();
                request.taskName = controller.taskNameInputText;
                request.taskDescription = controller.descriptionInfo;
                request.creationTime = poolTaskCellData.createTime;
                request.level = (TaskCategory)controller.classificationId;
                request.type = poolTaskCellData.type;
                request.language = Preference.scriptLanguage;

                request.attachs = K8AttachAndAttachSwitch.ToK8Attach(controller.attachDatas);

                if(request.creationTime == DateTime.MinValue) {
                    request.creationTime = DateTime.UtcNow;
                }
                request.blocking = true;
                request.Success(newId => {
                    if(controller.m_Mode == UITeacherEditTask.WorkMode.Edit_Pool_Mode) {
                        if(poolTaskCellData.name != controller.taskNameInputText || (int)poolTaskCellData.level != controller.classificationId) {
                            var delRequest = new DeleteRequest();

                            delRequest.type = TaskCommon.GetCatalog(poolTaskCellData.type, Preference.scriptLanguage);
                            delRequest.userId = UserManager.Instance.UserId;
                            delRequest.basePath = poolTaskCellData.id;

                            delRequest.Success((t) => {
                                controller.m_poolTaskUpdated(TaskPoolOperation.Remove, poolTaskCellData);
                                UpdatedPoolTask(request, newId, TaskPoolOperation.Add, poolTaskCellData);
                            })
                            .Error(() => {
                                UpdatedPoolTask(request, newId, TaskPoolOperation.Add, poolTaskCellData);
                            })
                                .Execute();
                        } else {
                            UpdatedPoolTask(request, newId, TaskPoolOperation.Update, poolTaskCellData);
                        }
                    } else {
                        UpdatedPoolTask(request, newId, TaskPoolOperation.Add, poolTaskCellData);
                    }
                    done();
                })
                       .Execute();
            });
        });

    }

    void UpdatedPoolTask(UploadTemplateTaskRequest request, string taskId, TaskPoolOperation op, TaskTemplate poolTaskCellData) {

        poolTaskCellData.id = taskId;
        poolTaskCellData.name = request.taskName;
        poolTaskCellData.description = request.taskDescription;
        poolTaskCellData.createTime = request.creationTime;
        poolTaskCellData.updateTime = ServerTime.UtcNow;
        poolTaskCellData.level = request.level;
        poolTaskCellData.attachs = request.attachs;
        controller.m_poolTaskUpdated(op, poolTaskCellData);
        PopupManager.Notice("ui_notice_save_succeeded".Localize());
    }

    void CheckDelProject(Action done) {
        var delProjects = controller.attachDatas.FindAll(x => { return x != null && x.state == AttachData.State.Delete && x.type == AttachData.Type.Project; });
        if(delProjects != null && delProjects.Count > 0) {
            var delTemplate = new DeletesRequest();
            foreach(var data in delProjects) {
                delTemplate.fullPaths.Add(data.webProgramPath);
            }
            delTemplate.Success((data) => {
                foreach(var del in delProjects) {
                    controller.attachDatas.Remove(del);
                }
                done();
            })
            .Execute();
        } else {
            done();
        }
    }
}
