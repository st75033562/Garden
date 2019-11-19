using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AttachData {
    public enum Type {
        Res,
        Project,
        Gameboard
    }
    public enum State {
        NewAdd,
        Initial,
        Delete
    }
    public Type type;
    public State state;
    public uint id;
    public LocalResData resData;
    public string programPath;
    public string programNickName;
    public string webProgramPath;
    public FileList webFileList;

    public bool isRelation;
    public bool hideDelReal;
    public uint itemId;
    public Gameboard.Gameboard gameboard;
}

public class PopupAttachmentManager : PopupController {
    public const int MaxAttachCount = 7;

    public class Payload {
        public List<AttachData> attachDatas;
        public Action<Action> onBeforeUploading;

        public int programCount;
        public int gameboardCount;
        public bool proGbMutex;
        public bool showResource;
        public bool hideDelReal;
        public int maxAttachCount ;
    }

    public GameObject progressGo;
    public ProgressBar progressBar;

    public Graphic[] m_replaceColorGraphics;
    public SelectResType selectResType;

    public ScrollLoopController scroll;
    public Button projectBtn;
    public Button gameboardBtn;
    public Button[] resBtns;

    private List<AttachData> attachDatas;
    private readonly List<LocalResData> attachments = new List<LocalResData>();
    private List<LocalResData> pendingVideos = new List<LocalResData>();
    private int uploadCount;
    private int uploadFinishedCount;
    private readonly Dictionary<HttpRequest, float> progresses = new Dictionary<HttpRequest, float>();
    private readonly List<HttpRequest> tasks = new List<HttpRequest>();
    private Payload attachConfig;

    public int robotCount { get; private set; }

    protected override void Start() {
        attachConfig = (Payload)payload;
        attachDatas = attachConfig.attachDatas;
        base.Start();
        foreach (Button btn in resBtns)
        {
            btn.gameObject.SetActive(attachConfig.showResource);
        }
        projectBtn.gameObject.SetActive(attachConfig.programCount > 0);
        gameboardBtn.gameObject.SetActive(attachConfig.gameboardCount > 0);

        progressBar.hint = "ui_uploading_attachments".Localize();
        selectResType.ListenResData((res)=> {
            attachments.Add(res);
            attachDatas.Insert(InsertFistIndex(), new AttachData { resData = res, type = AttachData .Type.Res});
            InitShowCell();
        });

        ResetRobotCount((count)=> {
            InitShowCell();
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach(var request in tasks) {
            request.Abort();
        }
        tasks.Clear();
    }

    public void OnClickProgram() {
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            PopupManager.ProjectView(projPath => {
                AddProject(projPath);
            }, showAddCell: false);
        } else {
            PopupManager.PythonProjectView(onSelected: (projPath) => {
                AddProject(projPath);
            }, programFolderOnly: false);
        }
    }

    int InsertFistIndex() {
        int index = attachDatas.Count - 1;
        if(index < 0) {
            index = 0;
        }
        return index;
    }

    public void OnClickAddGameBoard() {
        PopupGameBoardSelect.ConfigureParameter configure = new PopupGameBoardSelect.ConfigureParameter();
        configure.visibleType = PopupGameBoardSelect.VisibleType.All;
        configure.selectCallBack = (path) => {
            var gameboard = GameboardRepository.instance.getGameboard(path.ToString());
            if(gameboard != null) {
                gameboard.ClearCodeGroups();
                attachDatas.Insert(InsertFistIndex(), new AttachData {
                    programPath = path.ToString(),
                    type = AttachData.Type.Gameboard,
                    programNickName = gameboard.name,
                    gameboard = gameboard
                });

                robotCount = gameboard.robots.Count;
                InitShowCell();
            }
        };
        PopupManager.GameBoardSelect(configure);
    }

    void AddProject(IRepositoryPath projPath) {
        attachDatas.Insert(InsertFistIndex(), new AttachData {
            programPath = projPath.ToString(),
            type = AttachData.Type.Project,
            programNickName = projPath.name
        });
        InitShowCell();
    }

    void InitShowCell() {
        SwitchSelectBtns();
        var attachDataList = attachDatas.FindAll(x => x != null && x.state != AttachData.State.Delete);
        foreach (var attach in attachDataList)
        {
            attach.hideDelReal = attachConfig.hideDelReal;
        }
        scroll.initWithData(attachDataList);
    }

    public int GameboardCount {
        get {
            var attachDataList = attachDatas.FindAll(x => x != null && x.state != AttachData.State.Delete && x.type == AttachData.Type.Gameboard);
            return attachDataList.Count;
        }
    }

    void SwitchSelectBtns() {
        var attachDataList = attachDatas.FindAll(x => x != null && x.state != AttachData.State.Delete);
        if(attachDataList.Count >= attachConfig.maxAttachCount) {
            foreach(Button btn in resBtns) {
                btn.interactable = false;
            }
            projectBtn.interactable = false;
            gameboardBtn.interactable = false;
        } else {
            var programs = attachDatas.FindAll(x => { return x != null && x.type == AttachData.Type.Project && x.state != AttachData.State.Delete; });
            var gameboards = attachDatas.FindAll(x => { return x != null && x.type == AttachData.Type.Gameboard && x.state != AttachData.State.Delete; });
            foreach(Button btn in resBtns) {
                btn.interactable = true;
            }
            if(attachConfig.proGbMutex) {
                if(programs.Count > 0) {
                    gameboardBtn.interactable = false;
                    projectBtn.interactable = programs.Count < attachConfig.programCount;
                } else if(gameboards.Count > 0) {
                    projectBtn.interactable = false;
                    gameboardBtn.interactable = gameboards.Count < attachConfig.gameboardCount;
                } else {
                    gameboardBtn.interactable = true;
                    projectBtn.interactable = true;
                }
            } else {
                projectBtn.interactable = programs != null && programs.Count < attachConfig.programCount;
                gameboardBtn.interactable = gameboards != null && gameboards.Count < attachConfig.gameboardCount;
            }
        }    
    }

    private void UploadAttachments() {
        uploadCount = 0;
        uploadFinishedCount = 0;
        pendingVideos.Clear();

        progressGo.SetActive(true);
        UpdateProgress(0.0f);

        foreach(LocalResData res in attachments) {
            if(res == null ||
                Utils.IsValidUrl(res.name) ||
                (res.resType == ResType.Video && res.filePath == null) ||
                (res.resType == ResType.Image && res.textureData == null)) {
                continue;
            }

            uploadCount++;
            if(res.resType == ResType.Video) {
                pendingVideos.Add(res);
            } else {
                var request = Uploads.UploadMedia(res.textureData, res.name, false);
                request.finalHandler = () => { UploadResComplete(request, res); };
                request.uploadProgressHandler = GetProgressHandler(request);
                request.Execute();
            }
        }
        UploadVideo();

        if(uploadCount == 0) {
            UploadResComplete(null);
        }
    }

    void UploadVideo() {
        if(pendingVideos.Count > 0) {
            LocalResData res = pendingVideos[0];
            pendingVideos.RemoveAt(0);

            LoadResource.instance.LoadLocalRes(res.filePath, (www) => {

                var request = Uploads.UploadMedia(www.bytes, res.name, true);
                request.successHandler = delegate {
                    UploadResComplete(request);
                    UploadVideo();
                };
                request.errorHandler = () => UploadResComplete(request);
                request.uploadProgressHandler = GetProgressHandler(request);
                request.Execute();
                tasks.Add(request);
            });
        } else {
            GalleryUtils.RemoveTempFiles();
        }
    }
    private Action<float> GetProgressHandler(HttpRequest request) {
        return progress => {
            progresses[request] = progress;
            UpdateProgress(progresses.Values.Sum() / uploadCount);
        };
    }

    private void UpdateProgress(float progress) {
        progressBar.progress = progress;
    }

    private void UploadResComplete(HttpRequest request, LocalResData res = null) {
        if(res != null && res.resType == ResType.Course) {
            res.name = Singleton<WebRequestManager>.instance.GetMediaPath(res.name, false);
        }
        tasks.Remove(request);
        ++uploadFinishedCount;
        if(uploadFinishedCount >= uploadCount) {
            progressGo.SetActive(false);
            progresses.Clear();
            Close();
        }
    }

    public override void OnCloseButton() {
        foreach(AttachData content in attachDatas) {
            if(content == null) {
                continue;
            }
            string noticeStr = "";
            if(content.resData == null && string.IsNullOrEmpty(content.programNickName)) {
                noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_program".Localize());
                PopupManager.Notice(noticeStr);
                return;
            } else if(content.resData != null && string.IsNullOrEmpty(content.resData.nickName) && content.state != AttachData.State.Delete){
                if(content.resData != null) {
                    if(content.resData.resType == ResType.Course) {
                        noticeStr = string.Format("ui_text_name_no_empty".Localize(), "PDF");
                    } else if(content.resData.resType == ResType.Image) {
                        noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_task_type_image".Localize());
                    } else if(content.resData.resType == ResType.Video) {
                        noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_task_type_video".Localize());
                    }
                    PopupManager.Notice(noticeStr);
                    return;
                }
            }
        }
        if (attachConfig.onBeforeUploading != null)
        {
            attachConfig.onBeforeUploading.Invoke(UploadAttachments);
        }
        else
        {
            UploadAttachments();
        }
    }

    public void OnClickDel(AttachmentInfoCell data) {
        data.attachData.state = AttachData.State.Delete;
        attachments.Remove(data.attachData.resData);
        
        if(data.attachData.type == AttachData.Type.Gameboard) {
            ResetRobotCount((count) => {
                if(count == 0) {
                    foreach(var attachData in attachDatas) {
                        if(attachData != null)
                            attachData.isRelation = false;
                    }
                }
            });
        }
        InitShowCell();
    }

    void ResetRobotCount(Action<int> done = null) {
        var gameboardData = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard && x.state != AttachData.State.Delete; });
        if(gameboardData != null && gameboardData.gameboard != null) {
            robotCount = gameboardData.gameboard.robots.Count;
            if(done != null) {
                done(robotCount);
            }       
        } else if(gameboardData != null && !string.IsNullOrEmpty(gameboardData.webProgramPath)) {
            ProjectDownloadRequestV3 m_download = new ProjectDownloadRequestV3();
            m_download.basePath = gameboardData.webProgramPath;
            m_download
                .Success(dir => {
                    var project = dir.ToGameboardProject().gameboard;
                    robotCount = project.robots.Count;
                    gameboardData.gameboard = project;
                    if(done != null) {
                        done(robotCount);
                    }
                })
                .Execute();
        } else if(gameboardData != null && !string.IsNullOrEmpty(gameboardData.programPath)) {
            var gameboard = GameboardRepository.instance.getGameboard(gameboardData.programPath);
            if(gameboard != null) {
                robotCount = gameboard.robots.Count;
                gameboardData.gameboard = gameboard;
                if(done != null) {
                    done(robotCount);
                }
            }
        } else {
            robotCount = 0;
            if(done != null) {
                done(robotCount);
            }  
        }
    }

    public bool OnBeforeToggleCodeBinding(AttachData attachData, bool isOn) {
        if(isOn) {
            ResetRobotCount();
            var gbAttachs = attachDatas.FindAll(x => { return x != null && x.type == AttachData.Type.Gameboard && x.state != AttachData.State.Delete; });
            if(gbAttachs.Count > 1) {
                PopupManager.Notice("ui_text_gb_max_count".Localize());
                return false;
            }
            var relateAttachs = attachDatas.FindAll(x=> { return x != null && x.isRelation && x.state != AttachData.State.Delete; });
            if(relateAttachs.Count == robotCount) {
                PopupManager.Notice("ui_max_robot_code_selected".Localize(relateAttachs.Count));
                return false;
            }
        }
        attachData.isRelation = isOn;
        return true;
    }

    public IEnumerable<AttachData> GetAttachments()
    {
        return attachDatas.Where(x => x != null);
    }

}
