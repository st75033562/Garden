using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseExerciseStu : MonoBehaviour {

    public GameObject topAddGo;
    public GameObject addGo;
    public GameObject delGo;
    public ScrollLoopController scroll;
    public UISortMenuWidget uiSortMenuWidget;
    public GetTopicListType getTopicType;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name"
    };
    private UISortSetting sortSetting;
    public enum SortType {
        Name
    }

    private ShowLevel showLevel;
    private enum ShowLevel {
        ALL,
        LEVEL1,
        LEVEL2,
        LEVEL3
    }
    class AttachPack {
        public AttachData attachData;
        public string nickName;
    }
    void OnEnable() {
        topAddGo.SetActive(true);
        delGo.SetActive(false);
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(ExerciseTeaSettingStu.keyName, true);

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();
    }

    void SetSortMenu() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }
    protected virtual void OnDisable() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        RefreshView(true);
    }
    public void RefreshView(bool keepPostion) {
        SetSortMenu();

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if(comparer != null) {
            exerciseInfos.Sort(comparer);
        }
        UpdateExercise();
    }
    static Comparison<ExerciseInfo> GetComparison(int type, bool asc) {
        Comparison<ExerciseInfo> comp = null;
        switch((SortType)type) {
            case SortType.Name:
                comp = (x, y) => string.Compare(x.exerciesName, y.exerciesName, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }

    private List<ExerciseInfo> exerciseInfos = new List<ExerciseInfo>();

    void Start() {
        var getTopicList = new CMD_Get_Topic_List_r_Parameters();
        getTopicList.ReqType = GetTopicListType.GetTopicInvitedJoined;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            getTopicList.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            getTopicList.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        getTopicList.ReqType = getTopicType; ;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetTopicListR, getTopicList.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var topicList = CMD_Get_Topic_List_a_Parameters.Parser.ParseFrom(content);
                exerciseInfos.Clear();
                foreach(Topic_Info info in topicList.TopicList) {
                    ExerciseInfo exerciseInfo = new ExerciseInfo();
                    exerciseInfo.Parse(info);
                    exerciseInfos.Add(exerciseInfo);
                }
                RefreshView(false);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void UpdateExercise() {
        List<ExerciseInfo> showExercises = null;
        switch(showLevel) {
            case ShowLevel.ALL:
                showExercises = exerciseInfos;
                break;
            case ShowLevel.LEVEL1:
                showExercises = exerciseInfos.FindAll(x => { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL1; });
                break;
            case ShowLevel.LEVEL2:
                showExercises = exerciseInfos.FindAll(x => { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL2; });
                break;
            case ShowLevel.LEVEL3:
                showExercises = exerciseInfos.FindAll(x => { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL3; });
                break;
        }

        scroll.initWithData(showExercises);
        addGo.SetActive(exerciseInfos.Count == 0);
    }

    public void OnClickAdd() {
        if(!gameObject.activeInHierarchy) {
            return;
        }

        PopupAddExercise.Payload payload = new PopupAddExercise.Payload();

        if(getTopicType == GetTopicListType.GetTopicPublishedJoined) {
            payload.type = PopupAddExercise.Type.Publish;
        } else {
            payload.type = PopupAddExercise.Type.Test;
        }
        payload.addCallback = (topicInfo) => {
            ExerciseInfo exerciseInfo = new ExerciseInfo();
            exerciseInfo.Parse(topicInfo);
            exerciseInfos.Add(exerciseInfo);

            UserManager.Instance.AttendTopics.Add(topicInfo.TopicId, null);
            RefreshView(true);
        };

        PopupManager.AddExercise(payload);
    }

    public void OnClickCell(ExerciseCell cell) {
        int popupExerciseDetailId = 0;
        PopupExerciseDetail.Payload data = new PopupExerciseDetail.Payload();
        data.showDownBtn = true;
        data.exercieseInfo = cell.exerciseInfo;
        data.downloadBack = () => {
            DownLoadSave(cell, popupExerciseDetailId);
        };
        popupExerciseDetailId = PopupManager.ExerciseDetail(data);
    }

    void DownLoadSave(ExerciseCell cell, int popupExerciseDetailId) {
        List<IRepositoryPath> gbPaths = GameboardRepository.instance.listDirectory("");

        List<IRepositoryPath> projectPaths = null;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            projectPaths = CodeProjectRepository.instance.listDirectory("");
        } else {
            projectPaths = PythonRepository.instance.listDirectory("");
        }

        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            if(projectPaths.Find(x => { return x.ToString() == cell.exerciseInfo.exerciesName; }) != null) {
                PopupManager.Notice(string.Format("py_name_repeated".Localize(), cell.exerciseInfo.exerciesName));
                return;
            }
        }

        List<AttachData> attachDatas = cell.exerciseInfo.attachDatas;
        List<AttachPack> attachPacks = new List<AttachPack>();
        foreach(var data in attachDatas) {
            AttachPack attachPack = new AttachPack {
                attachData = data,
                nickName = data.programNickName
            };
            attachPacks.Add(attachPack);
            if(data.type == AttachData.Type.Gameboard) {
                ReName(attachPack, gbPaths);
            } else if(data.type == AttachData.Type.Project) {
                if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                    ReName(attachPack, projectPaths);
                }
            }
        }

        var copyAttach = new CMD_Get_Unique_Attach_r_Parameters();
        copyAttach.AttachUniqueId = cell.exerciseInfo.attachUniqueId;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            foreach(var data in attachPacks) {
                copyAttach.AttachSavedNames.Add(data.attachData.id, data.nickName);
            }
        } else {
            foreach(var data in attachPacks) {
                if(data.attachData.type == AttachData.Type.Project) {
                    copyAttach.AttachSavedNames.Add(data.attachData.id, cell.exerciseInfo.exerciesName);
                } else {
                    copyAttach.AttachSavedNames.Add(data.attachData.id, data.nickName);
                }
            }
        }

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetUniqueAttachR, copyAttach.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var attachA = CMD_Get_Unique_Attach_a_Parameters.Parser.ParseFrom(content);
                K8_Attach_Info info = attachA.AttachInfo;
                bool waitGbUpload = false;
                foreach(K8_Attach_Unit unit in info.AttachList.Values) {
                    if(unit.AttachType == K8_Attach_Type.KatProjects) {
                        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                            CodeProjectRepository.instance.save(unit.AttachName, unit.AttachFiles.FileList_);
                        } else {
                            PythonRepository.instance.save(cell.exerciseInfo.exerciesName, unit.AttachFiles.FileList_);
                        }
                    } else if(unit.AttachType == K8_Attach_Type.KatGameboard) {
                        GameboardRepository.instance.save(unit.AttachName, unit.AttachFiles.FileList_);
                        GameboardProject gb = unit.AttachFiles.ToGameboardProject();

                        RobotCodeGroups groups = null;
                        if(Preference.scriptLanguage == ScriptLanguage.Python) {
                            groups = gb.gameboard.GetCodeGroups(ScriptLanguage.Python);
                        } else {
                            groups = gb.gameboard.GetCodeGroups(ScriptLanguage.Visual);
                        }

                        List<AttachPack> relationAttach = new List<AttachPack>();
                        foreach(var group in groups) {
                            var remotePath = Gameboard.ProjectUrl.GetPath(group.projectPath);
                            var relation = attachPacks.Find(x => { return x.attachData.id.ToString() == remotePath; });
                            if(relation == null) {
                                Debug.LogError("not find relation id ");
                            } else {
                                relationAttach.Add(relation);
                            }
                        }

                        groups.ClearCodeGroups();
                        if(relationAttach.Count > 0) {
                            waitGbUpload = true;
                            for(int i = 0; i < relationAttach.Count; i++) {
                                var groupNew = new Gameboard.RobotCodeGroupInfo(relationAttach[i].nickName);
                                groupNew.Add(i);
                                groups.Add(groupNew);
                            }

                            var request = new UploadFileRequest();
                            request.type = GetCatalogType.GAME_BOARD_V2;
                            request.AddFile(unit.AttachName + "/" + GameboardRepository.GameBoardFileName, gb.gameboard.Serialize().ToByteArray());
                            request.Success(() => {
                                GameboardRepository.instance.save("", request.files.FileList_);
                                PopupManager.Notice("ui_download_sucess".Localize(), () => {
                                    PopupManager.Close(popupExerciseDetailId);
                                });
                                cell.DownLoadCount++;
                            })
                            .Execute();

                        } 
                    }
                }
                if(!waitGbUpload) {
                    cell.DownLoadCount++;
                    PopupManager.Notice("ui_download_sucess".Localize(), ()=> {
                        PopupManager.Close(popupExerciseDetailId);
                    });
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void ReName(AttachPack data, List<IRepositoryPath> paths) {
        int index = 0;
        string attachName = data.nickName;
        while(paths.Find(x => { return x.ToString() == attachName; }) != null) {
            attachName = data.nickName;
            attachName += "_" + index++;
        }
        data.nickName = attachName;
    }

    public void OnClickLevel(int level) {
        showLevel = (ShowLevel)level;
        UpdateExercise();
    }
}
