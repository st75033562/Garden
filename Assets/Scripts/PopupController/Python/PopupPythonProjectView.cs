using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PythonProjectViewConfig
{
    public Action<IRepositoryPath> onSelected; // non null for selection mode
    public string initialPath;
    public bool programFolderOnly; // folders which contain main.py
}

public class PopupPythonProjectView : PopupController {

    public ScrollLoopController scroll;
    public GameObject btnCancel;
    public Button[] disableModeBtns;
    public ButtonColorEffect btnBack;
    public GameObject btnFolderGo;
    public UISortMenuWidget uiSortMenuWidget;

    List<PythonData> pythonDatas = new List<PythonData>();

    System.Diagnostics.Process process;

    private IRepositoryPath currentPath;
    private int uploadPythonMaskId;
    private ApplicationQuitEvent quitEvent;
    private UISortSetting sortSetting;
    private bool isDeleting;
    private bool scrollViewInited;
    private HashSet<string> selectedPaths = new HashSet<string>();

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_modified_time",
        "ui_single_pk_sort_name"
    };

    // Use this for initialization
    protected override void Start () {
        base.Start();

        currentPath = PythonRepository.instance.createDirPath("");
        scroll.context = this;
        scroll.selectionModel.allowMultipleSelection = true;

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        RefreshOnDirChange();

        PythonScriptAutoUploader.instance.onFileUploaded += OnFileUploaded;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        PythonScriptAutoUploader.instance.onFileUploaded -= OnFileUploaded;
    }

    private void OnFileUploaded(string path)
    {
        if (Path.GetDirectoryName(path) == currentPath.ToString())
        {
            Refresh();
        }
    }

    void RefreshOnDirChange()
    {
        InitSortSetting();
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
        scroll.normalizedPosition = Vector2.one;
        scroll.selectionModel.ClearSelections();
        Refresh();
    }

    private PythonProjectViewConfig config
    {
        get { return (PythonProjectViewConfig)payload; }
    }

    public bool inSelectionMode
    {
        get { return config.onSelected != null; }
    }

    void InitCells() {
        SaveSelections();

        pythonDatas.Clear();

        var fileOpts = config.programFolderOnly ? FileListOptions.Directory : FileListOptions.FileOrDir;
        var fileInfoList = PythonRepository.instance.listFileInfos(currentPath.ToString(), fileOpts);

        foreach(var fileInfo in fileInfoList) {
            bool isProgramFolder = config.programFolderOnly && IsProgramFolder(fileInfo.path);

            var data = new PythonData();
            data.pathInfo = fileInfo;
            data.isProgramFolder = isProgramFolder;
            data.isDeleteState = isDeleting;
            pythonDatas.Add(data);
        }

        SortItem();
    }

    void SaveSelections()
    {
        selectedPaths.Clear();
        selectedPaths.UnionWith(
            scroll.selectionModel.selections
                  .Select(x => ((PythonData)x).pathInfo.path.ToString()));
    }

    bool IsProgramFolder(IRepositoryPath path)
    {
        bool foundMain = false;
        if (path.isDir)
        {
            var files = PythonRepository.instance.listDirectory(path.ToString());
            foreach (var f in files)
            {
                if (f.isFile && f.name == PythonConstants.Main)
                {
                    foundMain = true;
                    break;
                }
            }
        }
        return foundMain;
    }

    void SortItem() {
        var comparer = new PathInfoComparer((PathInfoMember)sortSetting.sortKey, sortSetting.ascending);
        pythonDatas.SortBy(x => x.pathInfo, comparer);
        if (!scrollViewInited)
        {
            scrollViewInited = true;
            scroll.initWithData(pythonDatas);
        }
        else
        {
            scroll.refresh();
        }

        RestoreSelections();
    }

    void RestoreSelections()
    {
        for (int i = 0; i < pythonDatas.Count; ++i)
        {
            if (selectedPaths.Contains(pythonDatas[i].pathInfo.path.ToString()))
            {
                scroll.selectionModel.Select(i, true);
            }
        }
    }

    public void ClickAdd() {
        PopupManager.InputDialog("ui_new_project".Localize(),
                                 "",
                                 "ui_input_new_project_hint".Localize(), 
                                 OnAddNewFile,
                                 new PythonNameValidator(currentPath.ToString(), "").ValidateInput);
    }

    void OnAddNewFile(string filename) {
        filename = FileUtils.ensureExtension(filename, ".py");
        var path = currentPath.AppendFile(filename).ToString();

        var request = new UploadFileRequest();
        request.type = GetCatalogType.PYTHON;
        request.AddFile(path, new byte[0]);
        request.Success(() => {
            PythonRepository.instance.save("", request.files.FileList_);
            Refresh();
            OpenEditor(filename);
        })
        .Execute();
    }

    void OpenEditor(string filename) {    
        string createFileName = FileUtils.ensureExtension(filename, ".py");
        var pythonPath = currentPath.AppendFile(createFileName).ToString();
        string path = PythonRepository.instance.getAbsPath(pythonPath);
        PythonEditorManager.instance.Open(path);
    }

    public void ClickdelCell(PythonData pythonData) {
        PythonScriptAutoUploader.instance.CancelUpload(pythonData.pathInfo.path.ToString());

        var request = new DeleteRequest();
        request.type = GetCatalogType.PYTHON;
        request.basePath = RequestUtils.EncodePath(pythonData.pathInfo.path.ToString());
        request.userId = UserManager.Instance.UserId;
        request.Success((t) => {
            PythonRepository.instance.delete(pythonData.pathInfo.path);
            var index = pythonDatas.FindIndex((x) => { return x.pathInfo.path.Equals(pythonData.pathInfo.path); });
            scroll.removeAt(index);
            selectedPaths.Remove(pythonData.pathInfo.path.ToString());
        })
        .Execute();
    }

    public void ClickCell(PythonData pythonData) {
        if (pythonData.pathInfo.path.isDir) {
            if (config.programFolderOnly && inSelectionMode && pythonData.isProgramFolder)
            {
                NotifySelected(pythonData.pathInfo.path);
                return;
            }

            currentPath = pythonData.pathInfo.path;
            RefreshOnDirChange();
        } else if (inSelectionMode) {
            NotifySelected(pythonData.pathInfo.path);
        }
    }

    private void NotifySelected(IRepositoryPath path)
    {
        config.onSelected(path);
        Close();
    }

    private void Refresh()
    {
        btnFolderGo.SetActive(currentPath.depth + 1 < ProjectRepository.MaxDepth);
        InitCells();
    }

    public void OnClickEditor(PythonData pythonData) {
        OpenEditor(pythonData.pathInfo.path.name);
    }

    public void OnClickPlay(PythonData pythonData) {
        string path = PythonRepository.instance.getAbsPath(pythonData.pathInfo.path.ToString());
        using (ScriptUtils.Run(path)) { }
    }

    public void OnClickFolder() {
        PopupManager.InputDialog("ui_new_folder".Localize(), "", "ui_input_new_folder_hint".Localize(),
            (str) => {
                var newFolderPath = currentPath.AppendLogicalDir(str.TrimEnd());

                var request = Uploads.CreateFolder(newFolderPath.ToString());
                request.type = GetCatalogType.PYTHON;
                request.Success(() => {
                    var dirInfo = PythonRepository.instance.createDirectory(newFolderPath.ToString(), request.creationTime);
                    var data = new PythonData {
                        pathInfo = new PathInfo(newFolderPath, dirInfo.creationTime, dirInfo.updateTime)
                    };

                    pythonDatas.Add(data);

                    InitCells();
                })
                .Execute();
            },
            new PythonFolderNameValidator(currentPath.ToString(), "", PythonRepository.instance).ValidateInput);
    }

    public void OnClickDel() {
        scroll.selectionModel.ClearSelections();
        selectedPaths.Clear();
        SetDeletingState(true);
    }

    void SetDeletingState(bool on)
    {
        SetDisableBtns(!on);
        isDeleting = on;
        foreach (PythonData data in pythonDatas)
        {
            data.isDeleteState = on;
        }
        scroll.refresh();
    }

    void SetDisableBtns(bool state) {
        foreach (Button btn in disableModeBtns)
        {
            btn.interactable = state;
        }
        btnBack.interactable = state;
        btnCancel.SetActive(!state);
    }

    public void OnClickCancel() {
        SetDeletingState(false);
    }

    public void OnClickSetting() {
        PopupManager.Settings();
    }

    public void OnClickBack() {
        if (currentPath.ToString() == "") {
            base.OnCloseButton();
        } else {
            currentPath = currentPath.parent;
            RefreshOnDirChange();
        }
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        SortItem();
    }

    void InitSortSetting() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(PythonSortSetting.GetKey(currentPath.ToString()), true);
    }
}
