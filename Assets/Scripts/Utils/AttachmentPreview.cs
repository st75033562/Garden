using Gameboard;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AttachInfo{
    public enum ResType{
        Resource,
        Program,
        GameBoard
    }
    public ResType resType;

    public LocalResData resData;
    public AttachProgram programData;
    public AttachGb gbData;
}

public class AttachProgram {
    public enum From{
        Local,
        Server
    }
    public From from;

    public string programPath;
}

public class AttachGb {
    public enum From {
        Local,
        Server
    }

    public From from;

    public string programPath;
    public string nickName;
    public bool openSave;
    public bool showEditButton = true;
    public string webRelationPath = "";
    public List<string> relations = null;
    public List<string> realRelations = null;
}


public class AttachmentPreview {

    public static void Preview(AttachInfo info) {
        switch(info.resType) {
            case AttachInfo.ResType.Resource:
                ResourcePv(info.resData);
                break;
            case AttachInfo.ResType.Program:
                ProgramPv(info.programData);
                break;
            case AttachInfo.ResType.GameBoard:
                GameBoardPv(info.gbData);
                break;
        }
    }

    static void ResourcePv(LocalResData localResData) {
        if(Utils.IsValidUrl(localResData.name)) {
            Application.OpenURL(localResData.name);
            return;
        }

        if(localResData.resType == ResType.Image) {
            if(localResData.textureData != null) {
                PopupManager.ImagePreview(localResData.textureData);
            } else {
                PopupManager.ImagePreview(localResData.name);
            }
        } else if(localResData.resType == ResType.Video) {
            PopupManager.VideoPlayer(Singleton<WebRequestManager>.instance.GetMediaPath(localResData.name, true));
        }
    }

    static void ProgramPv(AttachProgram data) {
        if(data.from == AttachProgram.From.Local) {
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                PopupManager.Workspace(CodeSceneArgs.FromPath(data.programPath));
            } else {
                PythonPreview.Preview(data.programPath);
            }
        } else {
            var request = new ProjectDownloadRequest();
            request.basePath = data.programPath;
            request.blocking = true;
            request.Success(tRt => {
                if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                    var project = tRt.ToProject();
                    if(project != null) {
                        PopupManager.Workspace(CodeSceneArgs.FromCode(project));
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

    static void GameBoardPv(AttachGb data) {
        ProjectPath path;
        if(data.from == AttachGb.From.Local) {
            path = ProjectPath.Local(data.programPath);
        } else {
            path = ProjectPath.Remote(data.programPath);
        }
        SaveHandler saveHandler = null;
        if(data.openSave) {
            saveHandler = () => {
                string proName = Path.GetFileName(data.nickName);
                if(GameboardRepository.instance.hasProject("", proName)) {
                    PopupManager.YesNo("local_down_notice".Localize(proName), () => {
                        ConfirmDownLoad(path.path, data.nickName);
                    });
                } else {
                    ConfirmDownLoad(path.path, data.nickName);
                }
            };
        }

        PopupManager.GameboardPlayer(path, editable: true, gameboardModifier: (gameboard) => {
            PatchGameboard(gameboard, data);
        }, saveHandler: saveHandler,
        relations: data.relations,
        showEditButton: data.showEditButton,
        isWebGb:!string.IsNullOrEmpty(data.webRelationPath));
    }

    static void PatchGameboard(Gameboard.Gameboard gameboard, AttachGb data) {
        if(data.realRelations != null) {
            var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
            groups.ClearCodeGroups();
            for(int i = 0; i < gameboard.robots.Count; ++i) {
                if(i < data.realRelations.Count) {
                    groups.SetRobotCode(i, data.realRelations[i]);
                } else {
                    break;
                }
            }
        }else if(data.from == AttachGb.From.Server) {
            var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
            foreach(var g in groups) {
                groups.ChangeGroupPath(g.projectPath, Gameboard.ProjectUrl.ToRemote(data.webRelationPath + Gameboard.ProjectUrl.GetPath(g.projectPath)));
            }
        }
    }

    static void ConfirmDownLoad(string path, string name) {
        var request = new ProjectDownloadRequestV3();
        request.blocking = true;
        request.basePath = path;
        request.Success(fileList => {
            GameboardRepository.instance.save(name, fileList.FileList_);
            GameboardProject gbProject = fileList.ToGameboardProject();
            gbProject.gameboard.ClearCodeGroups();
            var upload = Uploads.UploadGameboardV3(gbProject, name);
            upload.Success(() => {
                GameboardRepository.instance.save("", gbProject.ToFileNodeList(""));
                PopupManager.Notice("ui_notice_save_succeeded".Localize());
            })
            .Execute();
        })
        .Execute();
    }
}
