using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public enum GameBoardOperationType
{
    NONE,
    DELETE,
    CREATRFOLDER,
    PUBLISH,
    PUBLISH_BANK
}

public class GameBoardData
{
    // null for creating new gameboard cell
    public Gameboard.Gameboard gameBoard;
    public PathInfo pathInfo;
    public GameBoardOperationType operationType;
    public bool showSelectMask;
}

public class GameboardSelectResult
{
    // if templateId is not 0, path contains the parent directory of the new gameboard.
    // otherwise path is the selected gameboard path
    public int templateId { get; private set; }
    public IRepositoryPath path { get; private set; }


    public static GameboardSelectResult SelectExisting(IRepositoryPath path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (!path.isFile)
        {
            throw new ArgumentException("path is not a file");
        }
        return new GameboardSelectResult { path = path };
    }

    public static GameboardSelectResult NewGameboard(int templateId, IRepositoryPath path)
    {
        if (templateId == 0)
        {
            throw new ArgumentException("templateId");
        }
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (!path.isFile)
        {
            throw new ArgumentException("path is not a file");
        }
        return new GameboardSelectResult { templateId = templateId, path = path };
    }
}

// TODO: remove parameter `result'
public delegate void PopupGameboardResultCallback(GameboardSelectResult result);

public class PopupGameboardPayload
{
    public PopupGameboardResultCallback callback;
    public IRepositoryPath initialDir;
    public int initialThemeId; // if not 0, Theme popup is shown
}

public class PopupGameBoard : PopupController {
    [SerializeField]
    private ScrollLoopController scroll;
    [SerializeField]
    private Image deleteImage;

    public GameObject btnFolderGo;
    public Button[] disableModeBtns;
    public GameObject btnCancle;
    public ButtonColorEffect btnBack;
    public GameObject addItemGo;
    public UISortMenuWidget uiSortMenuWidget;
    public GameObject publishConfirmSubjectGo;
    public GameObject publishGo;
    public Button DeleteBtn;

    private PopupGameboardResultCallback callback;
    private List<GameBoardData> gameboardDataList;
    private IRepositoryPath currentPath;
    private GameBoardOperationType currentOpType = GameBoardOperationType.NONE;
    private List<GameBoardData> selectGbCells = new List<GameBoardData>();

    private const int maxUploadCount = 10;
    private UISortSetting sortSetting;
    private List<string> gbBankCatalogs;
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_modified_time",
        "ui_single_pk_sort_name"
    };

    protected override void Start() {
        var config = (PopupGameboardPayload)payload;
        callback = config.callback;

        if (config.initialDir != null && GameboardRepository.instance.isDirectory(config.initialDir))
        {
            currentPath = config.initialDir;
        }
        else
        {
            currentPath = GameboardRepository.instance.createDirPath("");
        }

        if (config.initialThemeId != 0)
        {
            ShowGameboardTheme(config.initialThemeId);
        }

        publishGo.SetActive(UserManager.Instance.IsAdmin);

        gameboardDataList = new List<GameBoardData>();
        scroll.context = this;

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        Refresh(true);
    }

    public void OnClickAdd() {
        ShowGameboardTheme(0);
    }

    private void ShowGameboardTheme(int initialThemeId)
    {
        PopupManager.GameBoardTheme((themeId) => {
            Close();
            if (callback != null)
            {
                callback(GameboardSelectResult.NewGameboard(themeId, currentPath.AppendFile("")));
            }
        }, initialThemeId);
    }

    public void OnClickCell(GameBoardCell cell) {
        if(cell.gameBoardData.pathInfo.path != null && cell.gameBoardData.pathInfo.path.isDir) {
            scroll.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
            currentPath = cell.gameBoardData.pathInfo.path;
            Refresh(true);
        } else {
            if(callback != null) {
                callback(GameboardSelectResult.SelectExisting(cell.gameBoardData.pathInfo.path));
            }
            Close();
        }
    }

    private void Refresh(bool reload)
    {
        if (reload)
        {
            gameboardDataList.Clear();
            foreach (var info in GameboardRepository.instance.listFileInfos(currentPath.ToString()))
            {
                var item = new GameBoardData();
                item.operationType = currentOpType;
                item.pathInfo = info;
                if (info.path.isFile)
                {
                    item.gameBoard = GameboardRepository.instance.getGameboard(info.path.ToString());
                    if (item.gameBoard == null)
                    {
                        continue;
                    }
                }
                gameboardDataList.Add(item);
            }
        }
        InitSortSetting();
        InitScroll();
        UpdateButtons();
    }

    private void UpdateButtons(Button activBtn = null)
    {
        btnFolderGo.SetActive(currentPath.depth + 1 < ProjectRepository.MaxDepth);

        bool editing = currentOpType != GameBoardOperationType.NONE;
        addItemGo.SetActive(gameboardDataList.Count == 0 && !editing);

        btnCancle.SetActive(editing);
        foreach (Button btn in disableModeBtns)
        {
            btn.interactable = !editing;
        }
        btnBack.interactable = !editing;
        if (activBtn != null) {
            activBtn.interactable = true;
        }
    }

    void InitScroll() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = new PathInfoComparer((PathInfoMember)sortSetting.sortKey, sortSetting.ascending);
        gameboardDataList.SortBy(x => x.pathInfo, comparer);
        scroll.initWithData(gameboardDataList);
    }

    public void OnClickShare(GameBoardCell cell) {
        var project = GameboardRepository.instance.loadGameboardProject(cell.gameBoardData.gameBoard.name);
        if (project != null)
        {
            int popupId = PopupManager.ShowMask();
            GameBoardSession gameBoardSession = new GameBoardSession();
            gameBoardSession.ShareGameBoard(project, (res, content)=> {
                PopupManager.Close(popupId);
                if(res == Command_Result.CmdNoError) {
                    var createGameboardA = CMD_Create_Gameboard_a_Parameters.Parser.ParseFrom(content);
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        }
        else
        {
            PopupManager.Notice("ui_failed_to_load_project".Localize()); 
        }
    }

    public void OnClickDelete(GameBoardCell cell) {
        var request = new DeleteRequest();
        request.type = GetCatalogType.GAME_BOARD_V2;
        request.basePath = RequestUtils.EncodePath(cell.gameBoardData.pathInfo.path.ToString());
        request.userId = UserManager.Instance.UserId;
        request.Success((t) => {
            GameboardRepository.instance.delete(cell.gameBoardData.pathInfo.path.ToString());
            gameboardDataList.Remove(cell.gameBoardData);
            InitScroll();
        })
            .Execute();

    }

    public override void OnCloseButton()
    {
        if(currentPath.ToString() == "") {
            base.OnCloseButton();

            if(callback != null) {
                callback(null);
            }
        } else {
            currentPath = currentPath.parent;
            Refresh(true);
        }
    }

    public void OnClickFolder() {
        var validator = new ProjectNameValidator(currentPath.ToString(), "", GameboardRepository.instance);
        PopupManager.InputDialog("ui_new_folder", "", "ui_input_new_folder_hint",
            (str) => {
                var newFolderPath = currentPath.AppendLogicalDir(str.TrimEnd());

                var request = Uploads.CreateFolder(newFolderPath.ToString());
                request.type = GetCatalogType.GAME_BOARD_V2;
                request.Success(() => {
                    var dirInfo = GameboardRepository.instance.createDirectory(newFolderPath.ToString(), request.creationTime);
                    var itemData = new GameBoardData {
                        pathInfo = new PathInfo(newFolderPath, dirInfo.creationTime, dirInfo.updateTime)
                    };

                    gameboardDataList.Add(itemData);

                    Refresh(false);
                })
                .Execute();
            },
            validator.ValidateInput);
    }

    public void OnClickOperation(int type) {
        currentOpType = (GameBoardOperationType)type;
        if (currentOpType == GameBoardOperationType.NONE)
        {
            foreach (var cell in selectGbCells)
            {
                cell.showSelectMask = false;
            }
            selectGbCells.Clear();
            publishConfirmSubjectGo.SetActive(false);
            UpdateButtons();
        }
        else if (currentOpType == GameBoardOperationType.DELETE) {
            UpdateButtons(DeleteBtn);
        }
        else if (currentOpType == GameBoardOperationType.PUBLISH_BANK)
        {
            UpdateButtons(publishGo.GetComponent<Button>());
            publishConfirmSubjectGo.SetActive(true);
            publishConfirmSubjectGo.GetComponent<Button>().interactable = selectGbCells.Count != 0;
        }
        else {
            UpdateButtons();
        }
        
        foreach (GameBoardData data in gameboardDataList)
        {
            data.operationType = currentOpType;
        }
        scroll.refresh();
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh(false);
    }

    void InitSortSetting() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(GameboardSortSetting.GetKey(currentPath.ToString()), true);
    }

    public bool addOrRemoveCell(GameBoardCell cell) {
        if (!selectGbCells.Contains(cell.gameBoardData))
        {
            if (selectGbCells.Count >= maxUploadCount) {
                PopupManager.Notice("gb_bank_uplaod_maxcount".Localize());
                return false;
            }
            selectGbCells.Add(cell.gameBoardData);
            publishConfirmSubjectGo.GetComponent<Button>().interactable = selectGbCells.Count != 0;
            cell.gameBoardData.showSelectMask = true;
            return true;
        }
        else {
            selectGbCells.Remove(cell.gameBoardData);
            publishConfirmSubjectGo.GetComponent<Button>().interactable = selectGbCells.Count != 0;
            cell.gameBoardData.showSelectMask = false;
            return false;
        }
    }

    public void OnClickConfirmPublishSubject()
    {
        PopupManager.GameBoardBank((data) =>
        {
            PublishSubject(data.path, data.password, data.catalogs);
        }, null);
    }

    public string ValidateInput(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (gbBankCatalogs.Contains(value))
        {
            return "name_already_in_use".Localize();
        }

        return null;
    }

    void LoadFile(FileList fileList, string path, int index, List<string> catalogs, Action done) {
        if (index >= selectGbCells.Count) {
            done();
            return;
        }
        GameBoardData gbCell = selectGbCells[index];
        if (catalogs.Contains(gbCell.gameBoard.name))
        {
            PopupManager.InputDialog("ui_text_rename".Localize(), gbCell.gameBoard.name, "",
            (str) =>
            {
                var project = GameboardRepository.instance.loadGameboardProject(gbCell.pathInfo.path.ToString());
                fileList.FileList_.Add(project.ToFileNodeList(path + str + "/"));
                LoadFile(fileList, path, ++index, catalogs, done);
            }, ValidateInput);
        }
        else
        {
            var project = GameboardRepository.instance.loadGameboardProject(gbCell.pathInfo.path.ToString());
            fileList.FileList_.Add(project.ToFileNodeList(path + gbCell.gameBoard.name + "/"));
            LoadFile(fileList, path, ++index, catalogs, done);
        }
    }

    void PublishSubject(string path, string passsword, List<string> catalogs) {
        gbBankCatalogs = catalogs;

        FileList fileList = new FileList();
        LoadFile(fileList, path, 0, catalogs, ()=> {
            Shared_Status status;
            if (passsword == null)
            {
                status = Shared_Status.Public;
            }
            else
            {
                status = Shared_Status.Invite;
            }

            int popupId = PopupManager.ShowMask();

            CMD_SharedData_Publish_r_Parameters pulishR = new CMD_SharedData_Publish_r_Parameters();
            pulishR.SharedStatus = status;
            if (passsword != null)
            {
                pulishR.SharedPassword = passsword;
            }
            pulishR.SharedData = fileList;

            SocketManager.instance.send(Command_ID.CmdShareddataPublishR, pulishR.ToByteString(), (res, content) =>
            {
                PopupManager.Close(popupId);
                if (res == Command_Result.CmdNoError)
                {
                    PopupManager.Notice("ui_published_sucess".Localize());
                    OnClickOperation(0);
                }
                else
                {
                    PopupManager.Notice(res.Localize());
                }
            });
        });
    }
}
