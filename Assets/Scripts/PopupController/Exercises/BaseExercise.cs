using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseExercise : MonoBehaviour {
    public ScrollLoopController scrollContent;
    public GameObject topAddGo;
    public GameObject topTestArea;
    public GameObject delGo;
    public GameObject topPublish;
    public UISortMenuWidget uiSortMenuWidget;

    private ShowLevel showLevel;
    private enum ShowLevel {
        ALL,
        LEVEL1,
        LEVEL2,
        LEVEL3
    }

    protected enum OperationState {
        Default,
        Delete,
        Test,
        Publish
    }

    protected OperationState operationState;

    protected List<ExerciseInfo> exerciseInfos = new List<ExerciseInfo>();
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };
    private UISortSetting sortSetting;
    public enum SortType {
        CreateTime,
        Name
    }

    public virtual ExerciseInfo GetExerciseByName(string name) {
        return null;
    }
    protected virtual void OnEnable() {
        delGo.SetActive(true);
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(ExerciseTeaSetting.keyName, true);

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
            case SortType.CreateTime:
                comp = (x, y) => x.createTime.CompareTo(y.createTime);
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.exerciesName, y.exerciesName, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }

    protected void SynchorExercise(GetTopicListType type) {
        var getTopicList = new CMD_Get_Topic_List_r_Parameters();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            getTopicList.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            getTopicList.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        getTopicList.ReqType = type;

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

    protected virtual void UpdateExercise() {
        List<ExerciseInfo> showExercises = null;
        switch(showLevel) {
            case ShowLevel.ALL:
                showExercises = exerciseInfos;
                break;
            case ShowLevel.LEVEL1:
                showExercises = exerciseInfos.FindAll(x=> { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL1; });
                break;
            case ShowLevel.LEVEL2:
                showExercises = exerciseInfos.FindAll(x => { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL2; });
                break;
            case ShowLevel.LEVEL3:
                showExercises = exerciseInfos.FindAll(x => { return (ShowLevel)(x.level + 1) == ShowLevel.LEVEL3; });
                break;
        }
        scrollContent.initWithData(showExercises); 
    }

    protected void ClickDel(ExerciseInfo exerciseInfo) {
        var delTopicR = new CMD_Del_Topic_r_Parameters();
        delTopicR.TopicId = exerciseInfo.id;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelTopicR, delTopicR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                exerciseInfos.Remove(exerciseInfo);
                UpdateExercise();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickDelMode() {
        if(gameObject.activeInHierarchy) {
            operationState = OperationState.Delete;
            SwitchMask(true);
        }
    }

    public virtual void OnClickCell(ExerciseCell exerciseCell) {
        if(operationState == OperationState.Delete) {
            PopupManager.YesNo("ui_confirm_delete".Localize(), () => {
                ClickDel(exerciseCell.exerciseInfo);
                SwitchMask(true);
            });
        } else {
            PopupExerciseDetail.Payload data = new PopupExerciseDetail.Payload();
            data.showDownBtn = false;
            data.exercieseInfo = exerciseCell.exerciseInfo;
            PopupManager.ExerciseDetail(data);
        }
    }

    public void OnClickLevel(int level) {
        showLevel = (ShowLevel)level;
        UpdateExercise();
    }

    public void SwitchMask(bool showMask) {
        foreach(ScrollCell cell in scrollContent.GetCellsInUse()) {
            cell.GetComponent<ExerciseCell>().ShowMask(showMask);
        }
        foreach(ExerciseInfo info in exerciseInfos) {
            info.showMask = showMask;
        }

        if(!showMask) {
            operationState = OperationState.Default;
        }
    }
}
