using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ExerciseInfo {
    public uint id;
    public Topic_Status status;
    public string exerciesName;
    public string exerciesDescripe;
    public uint price;
    public uint level;
    public uint userCount;
    public List<AttachData> attachDatas;
    public string attachUniqueId;
    public ulong createTime;
    public bool showMask;
    public uint downLoadCount;

    public const string serverTopicPath = "/download/topic/";
    public void Parse(Topic_Info info) {
        id = info.TopicId;
        status = info.TopicStatus;
        exerciesName = info.TopicName;
        exerciesDescripe = info.TopicDescription;
        price = info.TopicPrice;
        level = info.TopicLevel;
        userCount = info.TopicAttendUserCount;
        downLoadCount = info.TopicDownloadCount;
        createTime = info.TopicPublishTime;
        if(info.TopicAttachInfo != null) {
            attachDatas = K8AttachAndAttachSwitch.ToAttach(info.TopicAttachInfo, serverTopicPath + info.TopicId + "/");
            attachUniqueId = info.TopicAttachInfo.AttachUniqueId;
        } else {
            Debug.LogError("TopicAttachInfo is null");
        }
    }
}
public class PopupEditorExercises : PopupController {

    public static readonly string[] levelLanguare = new string[3]{ "ui_primary", "setting_level_2", "setting_level_3" };
    public class PayLoad {
        public ExerciseInfo exerciseInfo;
        public BaseExercise baseExercise;
    }

    public Dropdown dropdownLevel;
    public GameObject[] showUIs;
    public GameObject[] editorUIs;
    public ScrollLoopController attachmentScroll;
    public InputField inputCoin;
    public InputField inputName;
    public InputField inputDescribe;
    public Text textLevel;
    public Text textCoin;
    public Text textName;
    public Text textDescripe;
    public Button btnConfirm;
    private uint level;

    enum State {
        Show,
        Editor
    }

    private State currentState;
    private PayLoad configData;
    public List<AttachData> attachDatas = new List<AttachData>();

    protected override void Start () {
        configData = (PayLoad)payload;
        if(configData.exerciseInfo == null) {
            currentState = State.Editor;
        } else {
            AssignmentShowUi();
            SynchorBinding();
        }
        
        List<Dropdown.OptionData> levelOptionDatas = new List<Dropdown.OptionData>();
        levelOptionDatas.Add(new Dropdown.OptionData(PopupEditorExercises.levelLanguare[0].Localize()));
        levelOptionDatas.Add(new Dropdown.OptionData(PopupEditorExercises.levelLanguare[1].Localize()));
        levelOptionDatas.Add(new Dropdown.OptionData(PopupEditorExercises.levelLanguare[2].Localize()));

        dropdownLevel.options = levelOptionDatas;

        dropdownLevel.value = 2;
        SwitchUI();
        UpdateAttachScroll(currentState == State.Editor);
        SetConfirmBtn();
    }

    void SynchorBinding(Action done = null) {
        var gameboardAttach = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard; });
        if(gameboardAttach == null) {
            return;
        }
        var request = new SingleFileDownload();
        request.fullPath = gameboardAttach.webProgramPath + "/" + RequestUtils.Base64Encode(GameboardRepository.GameBoardFileName);
        request.Success(data => {
            Gameboard.Gameboard gameboard = Gameboard.Gameboard.Parse(data);
            gameboardAttach.gameboard = gameboard;
            var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
            foreach(AttachData res in attachDatas) {
                if(res == null || res.type != AttachData.Type.Project) {
                    continue;
                }
                var path = Gameboard.ProjectUrl.ToRemote(res.id.ToString());
                if(groups.GetGroup(path) != null) {
                    res.isRelation = true;
                }
            }
            if(done != null) {
                done();
            }
        })
        .Execute();
    }

    void SwitchUI() {
        
        bool isShowUI = currentState == State.Show;
        foreach (var go in showUIs)
        {
            go.SetActive(isShowUI);
        }
        foreach (var go in editorUIs)
        {
            go.SetActive(!isShowUI);
        }
        if(!isShowUI) {
            dropdownLevel.value = (int)level;
            inputCoin.text = textCoin.text;
            inputName.text = textName.text;
            inputDescribe.text = textDescripe.text;
        }
    }

    public void OnClickEditor() {
        currentState = State.Editor;
        SwitchUI();
        UpdateAttachScroll(true);
    }

    public void OnClickSave() {
        var exercise = configData.baseExercise.GetExerciseByName(inputName.text);
        if(exercise != null && exercise != configData.exerciseInfo) {
            PopupManager.Notice("text_repeat_task_name".Localize());
            return;
        }
        if(configData.exerciseInfo == null) {
            Create((gameboard)=> {
                UpdateGameBoardGroup(gameboard);
            });
        } else {
            Editor(()=> {
                UpdateGameBoardGroup();
            });
        }      
    }

    void UpdateGameBoardGroup(Gameboard.Gameboard gameboard = null) {
        var codeBindings = attachDatas.FindAll(x => { return x != null && x.isRelation; });
        AssignmentShowUi();
        var gameboardAttach = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard; });
        if(gameboard == null) {
            var request = new SingleFileDownload();
            request.fullPath = gameboardAttach.webProgramPath + "/" + RequestUtils.Base64Encode(GameboardRepository.GameBoardFileName);
            request.Success(data => {
                SetGameboardGroup(Gameboard.Gameboard.Parse(data), codeBindings, gameboardAttach);
            })
                .Execute();
        } else {
            SetGameboardGroup(gameboard, codeBindings, gameboardAttach);
        }
    }

    void SetGameboardGroup(Gameboard.Gameboard gameboard, List<AttachData> codeBindings, AttachData gameboardAttach) {
        RobotCodeGroups groups = null;
        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            groups = gameboard.GetCodeGroups(ScriptLanguage.Python);
        } else {
            groups = gameboard.GetCodeGroups(ScriptLanguage.Visual);
        }
        groups.ClearCodeGroups();
        for(int i = 0; i < codeBindings.Count; i++) {
            var remotePath = Gameboard.ProjectUrl.ToRemote(codeBindings[i].id.ToString());
            var group = new Gameboard.RobotCodeGroupInfo(remotePath);
            group.Add(i);
            group.projectName = codeBindings[i].programNickName;
            groups.Add(group);
        }
        UploadChangedItem(gameboard, gameboardAttach, () => {
            SwitchUI();
            UpdateAttachScroll(false);
            SynchorBinding();
        });
    }

    void UploadChangedItem(Gameboard.Gameboard gameboard, AttachData attach, Action done = null) {
        var updateAttachR = new CMD_Modify_Unique_Attach_r_Parameters();
        updateAttachR.AttachUniqueId = configData.exerciseInfo.attachUniqueId;

        var attachUnit = new K8_Attach_Unit();
        attachUnit.AttachName = attach.programNickName;
        attachUnit.AttachType = K8_Attach_Type.KatGameboard; ;
        attachUnit.AttachFiles = new FileList();

        FileNode tGb = new FileNode();
        tGb.PathName = GameboardRepository.GameBoardFileName;
        tGb.FileContents = gameboard.Serialize().ToByteString();
        attachUnit.AttachFiles.FileList_.Add(tGb);
        updateAttachR.ModifyInfo.Add(attach.id, attachUnit);

        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdModifyUniqueAttachR, updateAttachR.ToByteString(), (res, data) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                if(done != null) {
                    done();
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void AssignmentShowUi() {
        currentState = State.Show;
        level = configData.exerciseInfo.level;
        textLevel.text = levelLanguare[level].Localize();
        textCoin.text = configData.exerciseInfo.price.ToString();
        textName.text = configData.exerciseInfo.exerciesName;
        textDescripe.text = configData.exerciseInfo.exerciesDescripe;
        attachDatas = configData.exerciseInfo.attachDatas;
    }

    void Create(Action<Gameboard.Gameboard> done) {
        Topic_Info topicInfo = new Topic_Info();
        topicInfo.TopicStatus = Topic_Status.Commit;
        topicInfo.TopicName = inputName.text;
        topicInfo.TopicDescription = inputDescribe.text;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            topicInfo.TopicProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            topicInfo.TopicProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        uint coin = 0;
        uint.TryParse(inputCoin.text, out coin) ;
        topicInfo.TopicPrice = coin;
        topicInfo.TopicLevel = (uint)dropdownLevel.value;

        topicInfo.TopicAttachInfo = K8AttachAndAttachSwitch.ToK8Attach(attachDatas);

        CMD_Create_Topic_r_Parameters createTopicR = new CMD_Create_Topic_r_Parameters();
        createTopicR.CreateInfo = topicInfo;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdCreateTopicR, createTopicR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var topic = CMD_Create_Topic_a_Parameters.Parser.ParseFrom(content);
                configData.exerciseInfo = new ExerciseInfo();
                configData.exerciseInfo.Parse(topic.TopicInfo);

                var gameboard = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard; });
                if(gameboard != null) {
                    var gbProject = GameboardRepository.instance.loadGameboardProject(gameboard.programPath);
                    done(gbProject.gameboard);
                } else {
                    done(null);
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void Editor(Action done) {
        Topic_Info topicInfo = new Topic_Info();
        topicInfo.TopicId = configData.exerciseInfo.id;
        topicInfo.TopicName = inputName.text;
        topicInfo.TopicDescription = inputDescribe.text;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            topicInfo.TopicProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            topicInfo.TopicProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        uint coin = 0;
        uint.TryParse(inputCoin.text, out coin);
        topicInfo.TopicPrice = coin;
        
        topicInfo.TopicLevel = (uint)dropdownLevel.value;
        topicInfo.TopicAttachInfo = K8AttachAndAttachSwitch.ToK8AttachOnlyInfo(attachDatas);

        CMD_Modify_Topic_r_Parameters tUpdate = new CMD_Modify_Topic_r_Parameters();
        tUpdate.ModifyInfo = topicInfo;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdModifyTopicR, tUpdate.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Modify_Topic_a_Parameters tSuccess = CMD_Modify_Topic_a_Parameters.Parser.ParseFrom(content);
                configData.exerciseInfo.Parse(tSuccess.TopicInfo);

                UploadAddProgram((addTaskAttach) => {
                    if(addTaskAttach != null) {
                        foreach(uint key in addTaskAttach.AddInfo.Keys) {
                            configData.exerciseInfo.attachDatas.Add(K8AttachAndAttachSwitch.K8ToAttach(addTaskAttach.AddInfo[key], 
                                key, ExerciseInfo.serverTopicPath + configData.exerciseInfo.id + "/"));
                        }
                    }
                    DeleteServiceAttach((delTaskAttach) => {
                        if(delTaskAttach != null) {
                            foreach(uint delId in delTaskAttach.AttachIds) {
                                configData.exerciseInfo.attachDatas.RemoveAll(x=> x.id == delId);
                            }
                        }
                        done();
                    });
                });

            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void UploadAddProgram(Action<CMD_Add_Unique_Attach_a_Parameters> done) {  //task已经创建后，添加program
        var addAttach = new CMD_Add_Unique_Attach_r_Parameters();
        addAttach.AttachUniqueId = configData.exerciseInfo.attachUniqueId;

        foreach(AttachData res in attachDatas) {
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
                addAttach.AddInfo.Add(attachUnit);
            } else {
                var attachUnit = new K8_Attach_Unit();
                attachUnit.AttachName = res.programNickName;
                attachUnit.AttachType = K8AttachAndAttachSwitch.SwitchType(res);
                K8AttachAndAttachSwitch.PackFileList(attachUnit, res.programPath, res.type);
                addAttach.AddInfo.Add(attachUnit);
            }
        }

        if(addAttach.AddInfo.Count > 0) {
            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdAddUniqueAttachR, addAttach.ToByteString(), (res, content) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    done(CMD_Add_Unique_Attach_a_Parameters.Parser.ParseFrom(content));
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {
            done(null);
        }
    }

    void DeleteServiceAttach(Action<CMD_Del_Unique_Attach_a_Parameters> done) {
        var delAttach = new CMD_Del_Unique_Attach_r_Parameters();
        delAttach.AttachUniqueId = configData.exerciseInfo.attachUniqueId;
        foreach(AttachData res in attachDatas) {
            if(res == null || res.state != AttachData.State.Delete) {
                continue;
            }
            delAttach.AttachIds.Add(res.id);
        }
        if(delAttach.AttachIds.Count > 0) {
            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelUniqueAttachR, delAttach.ToByteString(), (res, content) => {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    done(CMD_Del_Unique_Attach_a_Parameters.Parser.ParseFrom(content));
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {
            done(null);
        }
    }

    void UpdateAttachScroll(bool showAdd) {
        if(showAdd) {
            if(attachDatas.Count == 0 || attachDatas[attachDatas.Count - 1] != null) {
                attachDatas.Add(null);
            }
        } else {
            attachDatas.Remove(null);
        }
        var effectiveAttach = attachDatas.FindAll(x => { return x == null || x.state != AttachData.State.Delete; });
        attachmentScroll.initWithData(effectiveAttach.Select(x => {
            return new AddAttachmentCellData(x, effectiveAttach.Where(y => y != null));
        }).ToArray());
    }

    public void OnClickAddAttach() {
        PopupManager.AttachmentManager(attachDatas, (done)=> {
            done();
        },
        ()=> {
            UpdateAttachScroll(true);
            SetConfirmBtn();
        },
        gameboardCount:1,
        showResouce: false);
    }

    public void SetConfirmBtn() {
        bool activeBtn = !string.IsNullOrEmpty(inputName.text);
        var effectAttach = attachDatas.Find(x => { return x != null && x.state != AttachData.State.Delete; });
        activeBtn = activeBtn && effectAttach != null;
        btnConfirm.interactable = activeBtn;
    }

    public void OnCoinChange() {
        inputCoin.text = inputCoin.text.Replace("-", "");
    }
}
