using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class PopupSinglePk : PopupController
{
    public Button m_buttonAdd;
    public Button m_buttonDelete;
    public Button m_buttonHome;
    public Button m_buttonBack;
    public Button m_buttonCancel;
    public Button m_buttonSort;

    public ScrollableAreaController m_scrollController;
    public GameObject m_emptyGo;

    private GameBoard m_selectedGameboard;
    private PopupUploadGameboard m_uploadPopup;
    private SinglePkGameboardListModel m_model;

    public UISortMenuWidget m_sortMenu;
    private int m_sortOption;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };

    private static readonly GBSort_Type[] s_sortTypes = {
        GBSort_Type.StData,
        GBSort_Type.StName
    };

    private UISortSetting m_sortSetting;

    protected GameObject _canvas;
    public void ShowCanvas(bool b)
    {
        if(_canvas != null)
        {
            _canvas.SetActive(b);
        }
    }

    // Use this for initialization
    protected override void Start()
    {
        LobbyManager manager = FindObjectOfType<LobbyManager>();
        if (manager != null)
        {
            _canvas = manager.m_Canvas.gameObject;
        }

        base.Start();

        m_model = new SinglePkGameboardListModel(4, 4);
        m_model.onItemInserted += OnItemInserted;

        m_scrollController.context = this;
        m_scrollController.InitializeWithData(m_model);

        m_buttonDelete.gameObject.SetActive(UserManager.Instance.IsAdmin);
        m_buttonAdd.gameObject.SetActive(UserManager.Instance.IsAdminOrTeacher);

        UpdateUI();

        m_sortSetting = SinglePkSortSetting.Get(UserManager.Instance);

        m_sortMenu.onSortChanged.AddListener(OnSortChanged);
        m_sortMenu.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        m_sortMenu.SetCurrentSort(SortTypeIndex(m_sortSetting.sortKey), m_sortSetting.ascending);
    }

    protected override void OnBackPressed()
    {
        ShowCanvas(true);
        base.OnBackPressed();
    }

    public override void OnCloseButton()
    {
        ShowCanvas(true);
        base.OnCloseButton();
    }

    private static int SortTypeIndex(int sortType)
    {
        return Array.IndexOf(s_sortTypes, (GBSort_Type)sortType);
    }

    private void OnSortChanged()
    {
        m_sortSetting.SetSortCriterion((int)s_sortTypes[m_sortMenu.activeSortOption], m_sortMenu.sortAsc);
        m_model.setSortCriterion(s_sortTypes[m_sortMenu.activeSortOption], m_sortMenu.sortAsc);
        m_model.fetchMore();

        m_scrollController.scrollPosition = 0.0f;
    }

    private void OnItemInserted(Range obj)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        m_emptyGo.SetActive(m_model.count == 0 && UserManager.Instance.IsAdminOrTeacher && !isDeleting);
        m_scrollController.Refresh();
        m_scrollController.gameObject.SetActive(m_model.count != 0);

        m_buttonAdd.interactable = !isDeleting;
        m_buttonHome.interactable = !isDeleting;
        m_buttonBack.interactable = !isDeleting;
        m_buttonSort.interactable = !isDeleting;
        m_buttonCancel.gameObject.SetActive(isDeleting);
    }

    public void OnClickCell(SinglePkCell cell)
    {
        if (isDeleting)
        {
            PopupManager.YesNo("ui_single_pk_delete_confirm".Localize(), () => {
                var request = new CMD_Del_Gameboard_r_Parameters();
                var gameboard = cell.data;
                request.GbId = gameboard.GbId;

                SocketManager.instance.send(Command_ID.CmdDelGameboardR, request.ToByteString(), (res, data) => {
                    if (!this) { return; }

                    if (res == Command_Result.CmdNoError || res == Command_Result.CmdGbNotFound)
                    {
                        m_model.removeItem(gameboard);
                        if (m_model.count == 0)
                        {
                            isDeleting = false;
                        }
                        UpdateUI();
                    }
                });
            });
        }
        else
        {
            m_selectedGameboard = (GameBoard)cell.DataObject;
            PopupManager.SinglePkDetail(m_selectedGameboard);
        }
    }

    public void OnClickDelete()
    {
        isDeleting = true;
        UpdateUI();
    }

    public void OnClickCancel()
    {
        isDeleting = false;
        UpdateUI();
    }

    private void UploadGameboard(GameboardUploadInfo uploadInfo)
    {
        var project = GameboardRepository.instance.loadGameboardProject(uploadInfo.gameboardPath);
        if (project == null)
        {
            PopupManager.Notice("ui_failed_to_load_project".Localize());
            m_uploadPopup.Close();
            m_uploadPopup = null;
            return;
        }
        if(uploadInfo.res != null) {
            Uploads.UploadMedia(uploadInfo.res.textureData, uploadInfo.res.name, false)
               .Blocking()
               .Success(() => {
                   UploadGameboard(uploadInfo, project, uploadInfo.res.name);
                   uploadInfo.res = null;
               })
               .Execute();
        } else {
            UploadGameboard(uploadInfo, project, null);
        }
        
    }

    void UploadGameboard(GameboardUploadInfo uploadInfo, GameboardProject project, string resName) {
        project.gameboard.ClearCodeGroups();
        project.gameboard.sourceCodeAvailable = uploadInfo.showSourceCode;

        var request = new CMD_Create_Gameboard_r_Parameters();
        request.GbName = uploadInfo.battleName;
        request.GbSenceId = (uint)project.gameboard.themeId;
        request.GbDescription = uploadInfo.description;
        request.GbParameters = new PkParameter {
            jsPointInfo = uploadInfo.startPointInfo,
            jsGBName = uploadInfo.gameboardName,
            jsPassMode = (int)uploadInfo.passModeType
        }.ToString();
        request.GbAllowRepeatAnswer = uploadInfo.allowRepeatChallenge;
        request.GbScriptShow = (uint)(uploadInfo.showSourceCode ? GbScriptShowType.Show : GbScriptShowType.Hide);

        request.GbFiles = new FileList();
        request.GbFiles.FileList_.AddRange(project.ToFileNodeList(""));

        if(!string.IsNullOrEmpty(resName)) {
            GB_Attach_Info attach = new GB_Attach_Info();
            attach.AttachUrlImage.Add(resName);
            request.GbAttachInfo = attach;
        }

        int popupId = PopupManager.ShowMask();
        GameBoardSession.SendShareGameBoard(request, (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                m_uploadPopup.Close();
                m_uploadPopup = null;

                var response = CMD_Create_Gameboard_a_Parameters.Parser.ParseFrom(content);
                m_model.addItem(response.GbInfo);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickAdd()
    {
        m_uploadPopup = PopupManager.UploadGameboard("ui_single_pk_upload_title".Localize(), UploadGameboard);
    }

    public bool isDeleting
    {
        get;
        private set;
    }
}
