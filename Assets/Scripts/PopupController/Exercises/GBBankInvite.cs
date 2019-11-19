using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GBBankInvite : GBBankShareBase
{
    public GameObject SearchPanel;
    public InputField inputField;
    
    protected override void OnEnable()
    {
        SharedStatus = Shared_Status.Invite;
        base.OnEnable();
        SearchPanel.SetActive(!UserManager.Instance.IsAdmin);
    }

    protected override void LoadListInfo()
    {
        if (UserManager.Instance.IsAdmin)
        {
            base.LoadListInfo();
        }
    }

    public void OnClickSearch() {
        if(string.IsNullOrEmpty(inputField.text)){
            return;
        }
        int popupId = PopupManager.ShowMask();
        CMD_SharedData_Getlist_r_Parameters getListR = new CMD_SharedData_Getlist_r_Parameters();
        getListR.SharedStatus = SharedStatus;
        getListR.ReqSharedName = inputField.text;
        FileCatalog.Clear();
        SocketManager.instance.send(Command_ID.CmdShareddataGetlistR, getListR.ToByteString(), (res, content) =>
        {
            PopupManager.Close(popupId);
            if (res == Command_Result.CmdNoError)
            {
                var getListA = CMD_SharedData_Getlist_a_Parameters.Parser.ParseFrom(content);
                if (getListA.SharedData != null)
                {
                    currentPath = "";
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

    public override void ShowMask(bool showMask)
    {
        foreach (var data in bankCellData)
        {
            data.showMask = showMask;
        }
        base.ShowMask(showMask);
    }

    public void OnClick(GBBankCell cell)
    {
        if (gameObject.activeSelf) {
            if ((currentPath == "" && cell.type == GBBankCell.Type.Folder) || operationType == OperationType.DOWNLOAD || operationType == OperationType.DELETE)
            {
                PopupManager.SetPassword("ui_input_the_password".Localize(), "", new SetPasswordData((str) =>
                {
                    base.OnClick(cell, str);
                }));
            }
            else
            {
                base.OnClick(cell, null);
            }
        }
    }
    
    public void OnClickCreateFloder()
    {
        if (gameObject.activeSelf)
        {
            if (currentPath == "")
            {
                PopupManager.SetPassword("ui_set_password".Localize(), "", new SetPasswordData((str) => {
                    base.OnClickAddFloder(str);
                }));
            }
            else
            {
                base.OnClickAddFloder(currentPassword);
            }
            
        }
    }
}
