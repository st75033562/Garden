using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Gameboard;

public abstract class BaseAttachmentCell : ScrollCell
{
    public Image defaultImage;
    public Sprite programIcon;
    public Sprite gameboardIcon;
    public ResourceIcons icons;

    public abstract AttachData attachData
    {
        get;
    }

    protected void initIcon() {
        var resData = attachData.resData;
        if(resData != null) {
            defaultImage.overrideSprite = icons.GetIcon(resData.resType);
        } else if(attachData.type == AttachData.Type.Project) {
            defaultImage.overrideSprite = programIcon;
        } else if(attachData.type == AttachData.Type.Gameboard) {
            defaultImage.overrideSprite = gameboardIcon; 
        }
    }

    public void OnClick() {
        if(attachData.type == AttachData.Type.Res) {
            var localResData = attachData.resData;
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
                if(string.IsNullOrEmpty(localResData.filePath)) {
                    PopupManager.VideoPlayer(Singleton<WebRequestManager>.instance.GetMediaPath(localResData.name, true));
                } else {
                    PopupManager.VideoPlayer("file://" + localResData.filePath);
                }
            }
        } else if(attachData.type == AttachData.Type.Project) {
            if(attachData.programPath != null) {
                if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                    PopupManager.Workspace(CodeSceneArgs.FromPath(attachData.programPath));
                } else {
                    PythonPreview.Preview(attachData.programPath);
                }
            } else {
                var request = new ProjectDownloadRequest();
                request.basePath = attachData.webProgramPath;
                request.blocking = true;
                request.Success(tRt => {
                    if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                        var project = tRt.ToProject();
                        if(project != null) {
                            OpenWorkspack(project);
                        } else {
                            Debug.LogError("project didn't download data");
                        }
                    } else {
                        PythonPreview.Preview(tRt);
                    }
                })
                .Execute();
            }
        } else if(attachData.type == AttachData.Type.Gameboard) {
            ProjectPath path;
            if(attachData.programPath != null) {
                path = ProjectPath.Local(attachData.programPath);
            } else {
                path = ProjectPath.Remote(attachData.webProgramPath);
            }
            GameboardPlayer(path, string.IsNullOrEmpty(attachData.programPath));
        }
    }

    protected virtual void OpenWorkspack(Project project) {
        PopupManager.Workspace(CodeSceneArgs.FromCode(project));
    }
    protected virtual void GameboardPlayer(ProjectPath path, bool isWebGb) {
        PopupManager.GameboardPlayer(path, gameboardModifier: PatchGameboard, relations: pythonFiles(), showEditButton: false, isWebGb: isWebGb, showBottomBts:false);
    }

    protected List<string> pythonFiles() {
        List<string> pys = new List<string>();
        foreach (var item in GetAttachments())
        {
            if(item != null && item.type == AttachData.Type.Project) {
                if(!string.IsNullOrEmpty(item.webProgramPath)) {
                    pys.Add(item.webProgramPath);
                }
            }
        }
        return pys;
    }

    protected void PatchGameboard(Gameboard.Gameboard gameboard)
    {
        var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
        groups.ClearCodeGroups();

        var boundProjects = GetAttachments().Where(x => x.isRelation).ToArray();
        for (int i = 0; i < gameboard.robots.Count; ++i)
        {
            if (i < boundProjects.Length)
            {
                string url;
                if (boundProjects[i].webProgramPath != null)
                {
                    url = ProjectUrl.ToRemote(boundProjects[i].webProgramPath);
                }
                else
                {
                    url = boundProjects[i].programPath;
                }

                Debug.Assert(url != null, "project path is null");
                groups.SetRobotCode(i, url);
            }
            else
            {
                break;
            }
        }
    }

    protected abstract IEnumerable<AttachData> GetAttachments();
}
