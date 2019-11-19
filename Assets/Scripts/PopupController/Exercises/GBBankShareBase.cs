using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GBBankShareBase : MonoBehaviour {
    public ScrollLoopController scroll;
    public UISortMenuWidget uiSortMenuWidget;
    protected Dictionary<string, List<FileNode>> FileCatalog = new Dictionary<string, List<FileNode>>();
    protected List<GBBankCell.Payload> bankCellData = new List<GBBankCell.Payload>();
    protected string currentPath = "";
    protected string currentPassword;

    public Action<PopupGameBoardBank.SelectData> selectPathBack;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };
    protected const string catalogName = "rootName/";
    public enum SortType
    {
        CreateTime,
        Name
    }
    protected enum OperationType
    {
        NONE,
        DOWNLOAD,
        DELETE
    }
    protected OperationType operationType;

    private UISortSetting sortSetting;

    protected Shared_Status SharedStatus;
    protected virtual void OnEnable()
    {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(BankCTSortSeeting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();

        LoadListInfo();
    }

    protected virtual void LoadListInfo() {
        int popupId = PopupManager.ShowMask();
        CMD_SharedData_Getlist_r_Parameters getListR = new CMD_SharedData_Getlist_r_Parameters();
        getListR.SharedStatus = SharedStatus;
        FileCatalog.Clear();
        SocketManager.instance.send(Command_ID.CmdShareddataGetlistR, getListR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if (res == Command_Result.CmdNoError)
            {
                var getListA = CMD_SharedData_Getlist_a_Parameters.Parser.ParseFrom(content);
                if (getListA.SharedData != null)
                {
                    foreach (var shareData in getListA.SharedData.FileList_)
                    {
                        if ((FN_TYPE)shareData.FnType != FN_TYPE.FnDir)
                        {
                            continue;
                        }
                        int index = shareData.PathName.LastIndexOf("/");
                        if (index == -1)
                        {
                            AddCatalog(catalogName, shareData);
                        }
                        else
                        {
                            AddCatalog(shareData.PathName.Substring(0, index + 1), shareData);
                        }
                    }
                }
                RefreshView(true);
            }
            else
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    protected void AddCatalog(string key, FileNode value) {
        if (FileCatalog.ContainsKey(key))
        {
            FileCatalog[key].Add(value);
        }
        else
        {
            List<FileNode> catalogs = new List<FileNode>();
            catalogs.Add(value);
            FileCatalog.Add(key, catalogs);
        }
    }
    void SetSortMenu()
    {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }

    private void OnSortChanged()
    {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        RefreshView(false);
    }

    public void RefreshView(bool clearData, bool showMask = false)
    {
        string key = currentPath == "" ? catalogName : currentPath;
        if (clearData) {
            bankCellData.Clear();
            if (!FileCatalog.ContainsKey(key))
            {
                scroll.initWithData(bankCellData);
                return;
            }

            foreach (var catalog in FileCatalog[key])
            {
                GBBankCell.Payload payload = new GBBankCell.Payload();
                payload.sharedInfo = catalog;
                payload.showMask = showMask;
                bankCellData.Add(payload);
            }
        }
        if (sortSetting != null) {
            SetSortMenu();
            var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
            if (comparer != null)
            {
                bankCellData.Sort(comparer);
            }
        }
        
        scroll.initWithData(bankCellData);
    }

    static Comparison<GBBankCell.Payload> GetComparison(int type, bool asc)
    {
        Comparison<GBBankCell.Payload> comp = null;
        switch ((SortType)type)
        {
            case SortType.CreateTime:
                comp = (x, y) =>
                {
                    if (x.sharedInfo.CreateTime.CompareTo(y.sharedInfo.CreateTime) != 0)
                    {
                        return x.sharedInfo.CreateTime.CompareTo(y.sharedInfo.CreateTime);
                    }
                    else
                    {
                        return string.Compare(x.sharedInfo.PathName, y.sharedInfo.PathName, StringComparison.CurrentCultureIgnoreCase);
                    }
                };
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.sharedInfo.PathName, y.sharedInfo.PathName, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }

    public void OnClickDownload()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        ShowMask(true);
        operationType = OperationType.DOWNLOAD;
    }

    public void OnClick(GBBankCell cell, string passWord)
    {
        if(passWord != null)
        {
            currentPassword = passWord;
        }
        if (operationType == OperationType.DOWNLOAD) {
            CanDownload(cell, (str) =>
            {
                int popupId = PopupManager.ShowMask();
                CMD_SharedData_Download_r_Parameters downloadR = new CMD_SharedData_Download_r_Parameters();
                downloadR.ReqPath = cell.ShareInfo.PathName;
                downloadR.ReqStatus = SharedStatus;
                downloadR.SaveasPath = str;
                if (passWord != null)
                {
                    downloadR.Password = passWord;
                }
                SocketManager.instance.send(Command_ID.CmdShareddataDownloadR, downloadR.ToByteString(), (res, content) =>
                {
                    PopupManager.Close(popupId);
                    if (res == Command_Result.CmdNoError)
                    {
                        var downloadA = CMD_SharedData_Download_a_Parameters.Parser.ParseFrom(content);
                        foreach (FileNode df in downloadA.SharedData.FileList_)
                        {
                            if ((FN_TYPE)df.FnType == FN_TYPE.FnDir)
                            {
                                string path = df.PathName;
                                if (path.StartsWith("/")) {
                                    path = df.PathName.Substring(1);
                                }
                                GameboardRepository.instance.createDirectory(path, TimeUtils.FromEpochSeconds((long)df.CreateTime));
                            }
                        }
                        if (string.IsNullOrEmpty(downloadA.SharedData.RootPath) || downloadA.SharedData.RootPath == "/")
                        {
                            GameboardRepository.instance.save("", downloadA.SharedData.FileList_);
                        }
                        else
                        {
                            GameboardRepository.instance.save(downloadA.SharedData.RootPath, downloadA.SharedData.FileList_);
                        }

                        PopupManager.Notice("ui_download_sucess".Localize());
                    }
                    else
                    {
                        PopupManager.Notice(res.Localize());
                    }
                });
            });
        } else if (operationType == OperationType.NONE) {
            if (currentPath == "" && SharedStatus == Shared_Status.Invite && cell.type == GBBankCell.Type.Folder) {
                CMD_SharedData_Check_PW_r_Parameters checkPw = new CMD_SharedData_Check_PW_r_Parameters();
                checkPw.SharedStatus = SharedStatus;
                checkPw.ReqPath = cell.ShareInfo.PathName;
                if (passWord != null) {
                    checkPw.Password = passWord;
                }
                int popupId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdShateddataCheckPwR, checkPw.ToByteString(), (res, content) =>
                {
                    PopupManager.Close(popupId);
                    if (res == Command_Result.CmdNoError)
                    {
                        var checkoutA = CMD_SharedData_Check_PW_a_Parameters.Parser.ParseFrom(content);
                        if (checkoutA.CheckResult == 0) {
                            InterFloder(cell);
                        }
                        else {
                            PopupManager.Notice("ui_password_incorrect".Localize());
                        }
                    }
                    else
                    {
                        PopupManager.Notice(res.Localize());
                    }
                });
            }
            else
            {
                InterFloder(cell);
            }
        } else if (operationType == OperationType.DELETE) {
            PopupManager.YesNo("ui_text_remove_problem".Localize(), () => {
                int popupId = PopupManager.ShowMask();
                CMD_SharedData_Del_r_Parameters deleteR = new CMD_SharedData_Del_r_Parameters();
                deleteR.ReqPath = cell.ShareInfo.PathName;
                deleteR.ReqStatus = SharedStatus;
                if (passWord != null)
                {
                    deleteR.SharedPassword = passWord;
                }
                SocketManager.instance.send(Command_ID.CmdShareddataDelR, deleteR.ToByteString(), (res, content) =>
                {
                    PopupManager.Close(popupId);
                    if (res == Command_Result.CmdNoError)
                    {
                        string key = currentPath == "" ? catalogName : currentPath;
                        FileCatalog[key].Remove(cell.ShareInfo);
                        
                        string key1 = currentPath + cell.ShareInfo.PathName +  "/" ;
                        
                        if (cell.type == GBBankCell.Type.Folder)
                        {
                            List<string> delKeys = new List<string>();
                            foreach (var key2 in FileCatalog.Keys) {
                                if (key2.StartsWith(key1)) {
                                    delKeys.Add(key2);
                                }
                            }
                            foreach (var delKey in delKeys) {
                                FileCatalog.Remove(delKey);
                            }
                        }
                        RefreshView(true, true);
                    }
                    else
                    {
                        PopupManager.Notice(res.Localize());
                    }
                });
            }, null);
        }
    }

    void InterFloder(GBBankCell cell) {
        if (cell.type == GBBankCell.Type.Folder)
        {
            currentPath = cell.ShareInfo.PathName;
            if (!currentPath.EndsWith("/"))
            {
                currentPath += "/";
            }
            RefreshView(true);
        }
    }

    protected void CanDownload(GBBankCell cell,Action<string> done) {
        string fullPath = cell.ShareInfo.PathName.ToLower();
        int indexOf = fullPath.LastIndexOf("/");
        string path = "";
        string fileName = fullPath;
        if (indexOf != -1) {
            path = fullPath.Substring(0, indexOf + 1);
            fileName = fullPath.Substring(indexOf + 1);
        }

        if (fileName.StartsWith(CodeProjectRepository.FolderPrefix)) {
            fileName = fileName.Substring(CodeProjectRepository.FolderPrefix.Length);
        }
        if (Directory.Exists(GameboardRepository.instance.getAbsPath(path)) && GameboardRepository.instance.existsPath(path, fileName))
        {
            var validator = new ProjectNameValidator(currentPath.ToString(), "", GameboardRepository.instance);
            PopupManager.InputDialog("ui_text_rename", fileName, "ui_input_new_folder_hint",
                (str) =>
                {
                    if (cell.type == GBBankCell.Type.Folder) {
                        done(path + CodeProjectRepository.FolderPrefix + str);
                    }
                    else {
                        done(path + str);
                    }
                },
                validator.ValidateInput);
        }
        else
        {
            done(cell.ShareInfo.PathName);
        }
    }

    public virtual void ShowMask(bool showMask)
    {
        RefreshView(false);
    }

    protected void OnClickAddFloder(string passWord) {
        if (!gameObject.activeSelf) {
            return;
        }
        PopupManager.InputDialog("ui_new_folder".Localize(), "", "class_task_name_hint".Localize(),
            (str) => {
                Debug.Log(currentPath);
                string finalName = currentPath + CodeProjectRepository.FolderPrefix + str;
                int popupId = PopupManager.ShowMask();
                CMD_SharedData_CreateDir_r_Parameters CreateDirR = new CMD_SharedData_CreateDir_r_Parameters();
                CreateDirR.CreateDir = finalName;
                CreateDirR.SharedStatus = SharedStatus;
                if (passWord != null) {
                    CreateDirR.SharedPassword = passWord;
                    currentPassword = passWord;
                }

                SocketManager.instance.send(Command_ID.CmdShareddataCreatedirR, CreateDirR.ToByteString(), (res, content) =>
                {
                    PopupManager.Close(popupId);
                    if (res != Command_Result.CmdNoError)
                    {
                        PopupManager.Notice(res.Localize());
                    }
                    else
                    {
                        string key = currentPath == "" ? catalogName : currentPath;
                        FileNode node = new FileNode();
                        node.PathName = finalName;
                        node.FnType = (int)FN_TYPE.FnDir;
                        node.CreateTime = (ulong)ServerTime.UtcNow.ToFileTimeUtc();
                        AddCatalog(key, node);
                    //    FileCatalog[key].Add(node);
                        RefreshView(true);
                    }
                });
                Debug.Log(finalName);
            },
            ValidateInput);
    }

    string ValidateInput(string value) {
        string result = FolderNameVerify.Verify(value);
        if (string.IsNullOrEmpty(result))
        {
            foreach (var data in bankCellData)
            {
                string finalValue = value;
                if ((FN_TYPE)data.sharedInfo.FnType == FN_TYPE.FnDir)
                {
                    finalValue = CodeProjectRepository.FolderPrefix + value;
                }
                if (Path.GetFileName(data.sharedInfo.PathName) == finalValue)
                {
                    return "name_already_in_use".Localize();
                }
            }
        }
        else
        {
            return result;
        }
        return null;
    }

    public void OnClickDeleteMode() {
        if (gameObject.activeSelf) {
            operationType = OperationType.DELETE;

            foreach (var data in bankCellData)
            {
                data.showMask = true;
            }

            RefreshView(false);
        }
    }

    public void OnClickConfirmSelect() {
        
        if (gameObject.activeSelf) {
            if (currentPath == "" && SharedStatus == Shared_Status.Invite)
            {
                PopupManager.SetPassword("ui_set_password".Localize(), "", new SetPasswordData((str) =>
                {
                    currentPassword = str;
                    ConfirmSelect();
                }));
            }
            else
            {
                ConfirmSelect();
            }
        }
    }

    void ConfirmSelect() {
        PopupGameBoardBank.SelectData data = new PopupGameBoardBank.SelectData();
        data.path = currentPath;
        data.password = currentPassword;
        data.catalogs.Clear();
        string key = currentPath == "" ? catalogName : currentPath;
        if (FileCatalog.ContainsKey(key))
        {
            foreach (var catalog in FileCatalog[key])
            {
                data.catalogs.Add(Path.GetFileName(catalog.PathName));
            }
        }
        selectPathBack(data);
    }

    public bool Close() {
        if (currentPath == "") {
            return true;
        }
        string[] strs = currentPath.Split('/');
        if (strs.Length > 2)
        {
            currentPath = strs[0] + "/";
            for (int i = 1; i < strs.Length - 2; i++)
            {
                currentPath += strs[i] + "/";
            }
        }
        else {
            currentPath = "";
        }
        RefreshView(true);
        return false;
    }

    public void OnClickCancle() {
        operationType = OperationType.NONE;
        ShowMask(false);
    }
}
