//#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class ClassShowInfoData {
    public MyClassController classController;
    public ClassInfo banji;
}

public class MyClassController : MonoBehaviour {
    public GameObject[] hideBtns;

    [SerializeField]
    private ScrollLoopController scrollController;
    [SerializeField]
    private GameObject goAddClass;
    public UISortMenuWidget uiSortMenuWidget;
    private UISortSetting sortSetting;
    private List<ClassShowInfoData> classShowInfos;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name"
    };

    public enum SortType {
        Name,
    }

    private IKickNotificationEvent m_kickNotificationEvent;

    // Use this for initialization
    void Start () {
        InitKickNotificationEvent();
    }

    void InitKickNotificationEvent()
    {
#if TEST
        m_kickNotificationEvent = gameObject.AddComponent<TestKickNotificationService>();
#else
        m_kickNotificationEvent = gameObject.AddComponent<KickNotificationHelper>();
#endif
        m_kickNotificationEvent.onCompleteNotifications += OnCompleteKickNotifications;
        m_kickNotificationEvent.onDidConfirmNotification += OnDidConfirmKickNotification;
    }

    void OnEnable() {
        foreach (GameObject go in hideBtns)
        {
            go.SetActive(false);
        }

    //    goAddClass.SetActive(GetJoinedClasses().Count() == 0);

        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(MyClassSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();

        Project_Language_Type languageType = Preference.scriptLanguage == ScriptLanguage.Visual ?
            Project_Language_Type.ProjectLanguageGraphy : Project_Language_Type.ProjectLanguagePython;

        int popId = PopupManager.ShowMask();
        NetManager.instance.GetAllClassInfos((result) => {
            if(result == Command_Result.CmdNoError) {
                RefreshClasses();
            } else {
                PopupManager.Notice(result.Localize());
            }
            PopupManager.Close(popId);
        }, languageType);
    }
    void OnDisable() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
    }

    IEnumerable<ClassInfo> GetJoinedClasses() {
        return UserManager.Instance.ClassList.FindAll(
            banji => banji.languageType == Preference.scriptLanguage 
                && banji.m_ClassStatus == ClassInfo.Status.Attend_Status);
    }

    void SetSortMenu() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        if(sortSetting.sortKey > s_sortOptions.Length - 1) {
            sortSetting.Reset();
            Debug.LogError("sortSetting sortKey error");
        }
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }

    private void RefreshClasses()
    {
        SetSortMenu();
        GetJoinedClasses();
        var scrollData = GetJoinedClasses()
                            .Select(x => new ClassShowInfoData {
                                classController = this,
                                banji = x
                            })
                            .ToList();

        goAddClass.SetActive(scrollData.Count == 0);
        Refresh(scrollData);
    }

    void Refresh(List<ClassShowInfoData> data) {
        classShowInfos = data;

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            classShowInfos.Sort(comparer);
        }
        scrollController.initWithData(classShowInfos);
    }

    public void OnClickAdd () {
        if(gameObject.activeInHierarchy) {
            PopupManager.AddClass();
        }
    }

    public void OnItemClick (ClassInfo banjiInfo) {
        UserManager.Instance.CurClass = banjiInfo;
        PopupManager.StudentTasks(m_kickNotificationEvent);

#if TEST
        var service = (TestKickNotificationService)m_kickNotificationEvent;
        service.SetCurrentClass(banjiInfo.m_ID, banjiInfo.m_Name);
#endif
    }

    void OnDidConfirmKickNotification(KickNotification notification) {
        UserManager.Instance.DeleteClass(notification.classId);
    }

    void OnCompleteKickNotifications() {
        RefreshClasses();
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh(classShowInfos);
    }

    static Comparison<ClassShowInfoData> GetComparison(int type, bool asc) {
        switch((SortType)type) {
            case SortType.Name:
                return (x, y) => {
                    var res = string.Compare(x.banji.m_Name, y.banji.m_Name, StringComparison.CurrentCultureIgnoreCase);
                    return asc ? res : -res;
                };
        }
        return null;
    }
}
