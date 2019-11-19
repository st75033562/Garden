using Google.Protobuf;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class PkController : StackUIBase {
    [SerializeField]
    private ScrollableAreaController pkScroll;
    [SerializeField]
    private PkDetail pkDetail;
    [SerializeField]
    private GameObject deleteButtonGo;

    [SerializeField]
    private ButtonColorEffect backButton;

    [SerializeField]
    private Button addButton;

    [SerializeField]
    private Button homeButton;

    [SerializeField]
    private GameObject cancelButtonGo;

    [SerializeField]
    private GameObject emtpyGo;

    public UISortMenuWidget sortMenu;

    private int maskId;
    private PopupUploadGameboard m_popupUploadGamboard;
    private DoublePlayerPkListModel m_model;
    private bool isDeleting;
    private UISortSetting sortSetting;

    private static readonly string[] s_sortOptions = {
        "ui_multi_pk_sort_creation_time",
        "ui_multi_pk_sort_name"
    };

    private static readonly PKSort_Type[] s_sortTypes = {
        PKSort_Type.PkStData,
        PKSort_Type.PkStName
    };

    public override void OpenWindow(bool isPush)
    {
        base.OpenWindow(isPush);

        if (isPush)
        {
            isDeleting = false;

            deleteButtonGo.SetActive(UserManager.Instance.IsAdmin);
            addButton.gameObject.SetActive(UserManager.Instance.IsAdminOrTeacher);

            m_model = new DoublePlayerPkListModel();
            m_model.onReset += UpdateUI;
            m_model.onItemInserted += OnItemInserted;

            pkScroll.InitializeWithData(m_model);
            pkScroll.context = this;

            sortSetting = PKSortSetting.Get(UserManager.Instance);

            sortMenu.onSortChanged.AddListener(OnSortChanged);
            sortMenu.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
            sortMenu.SetCurrentSort(SortTypeIndex(sortSetting.sortKey), sortSetting.ascending);

            UpdateUI();
        }
    }

    public override void CloseWindow(bool isPush)
    {
        base.CloseWindow(isPush);

        if (!isPush)
        {
            m_model.onReset -= UpdateUI;
            m_model.onItemInserted -= OnItemInserted;
            m_model = null;
        }
    }

    private static int SortTypeIndex(int type)
    {
        return Array.IndexOf(s_sortTypes, (PKSort_Type)type);
    }

    private void OnSortChanged()
    {
        sortSetting.SetSortCriterion((int)s_sortTypes[sortMenu.activeSortOption], sortMenu.sortAsc);
        m_model.setSortCriterion(s_sortTypes[sortMenu.activeSortOption], sortMenu.sortAsc);
        m_model.fetchMore();
        pkScroll.scrollPosition = 0.0f;
    }

    private void OnItemInserted(Range range)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        backButton.interactable = !isDeleting;
        addButton.interactable = !isDeleting;
        homeButton.interactable = !isDeleting;
        deleteButtonGo.SetActive(UserManager.Instance.IsAdmin);
        cancelButtonGo.SetActive(isDeleting);
        emtpyGo.SetActive(m_model.count == 0 && !isDeleting && UserManager.Instance.IsAdminOrTeacher);
    }

    public void ClickAdd() {
        m_popupUploadGamboard = PopupManager.UploadGameboard(
            "ui_new_battle".Localize(), 
            UploadPkScript,
            gb => gb.robots.Count >= 2);
    }

    public void OnClickCell(PkCell cell) {
        if (isDeleting)
        {
            PopupManager.YesNo("ui_pk_delete_battle_confirm".Localize(),
                () => DeletePk(cell.pkData));
        }
        else
        {
            pkDetail.gameObject.SetActive(true);
            pkDetail.SetData(cell.pkData);
        }
    }

    private void DeletePk(PK data)
    {
        var pkDelR = new CMD_Del_PK_r_Parameters();
        pkDelR.PkId = data.PkId;
        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelPkR, pkDelR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if (res == Command_Result.CmdNoError)
            {
                m_model.removeItem(data);
                if (isDeleting && m_model.count == 0)
                {
                    EndDelete();
                }
            }
            else
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public bool IsDeleting
    {
        get { return isDeleting; }
    }

    public void BeginDelete()
    {
        SetDeleteMode(true);
    }

    public void EndDelete()
    {
        SetDeleteMode(false);
    }

    private void SetDeleteMode(bool on)
    {
        isDeleting = on;
        pkScroll.Refresh();
        UpdateUI();
    }

    void UploadPkScript(GameboardUploadInfo uploadInfo) {
        var project = GameboardRepository.instance.loadGameboardProject(uploadInfo.gameboardPath);
        if (project == null)
        {
            PopupManager.Notice("ui_failed_to_load_project".Localize());
            Debug.LogError("failed to load project");
            return;
        }

        maskId = PopupManager.ShowMask();
       
        var pk_r = new CMD_Create_PK_r_Parameters();
        pk_r.PkName = uploadInfo.battleName;
        pk_r.PkSenceId = (uint)project.gameboard.themeId;
        pk_r.PkDescription = uploadInfo.description;

        pk_r.PkParameters = new PkParameter {
            jsPointInfo = uploadInfo.startPointInfo,
            jsGBName = uploadInfo.gameboardName
        }.ToString();

        pk_r.PkScriptShow = uploadInfo.showSourceCode ? (uint)GbScriptShowType.Show : (uint)GbScriptShowType.Hide;
        pk_r.PkAllowRepeatAnswer = uploadInfo.allowRepeatChallenge ? 0u : 1u;

        project.gameboard.ClearCodeGroups();
        project.gameboard.sourceCodeAvailable = uploadInfo.showSourceCode;
        pk_r.PkFiles = new FileList();
        pk_r.PkFiles.FileList_.AddRange(project.ToFileNodeList(""));

        SocketManager.instance.send(Command_ID.CmdCreatePkR, pk_r.ToByteString(), OnCreatePk);
    }

    void OnCreatePk(Command_Result res, ByteString content) {
        PopupManager.Close(maskId);
        if(res == Command_Result.CmdNoError) {
            var pk_a = CMD_Create_PK_a_Parameters.Parser.ParseFrom(content);
            m_model.addItem(pk_a.PkInfo);
            PopupManager.Close(m_popupUploadGamboard);
        } else {
            PopupManager.Notice(res.Localize());
        }
    }

    public void OnClickBack() {
        StackUIBase.Pop();
    }
}
