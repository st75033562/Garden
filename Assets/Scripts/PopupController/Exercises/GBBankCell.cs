using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GBBankCell : ScrollCell
{
    public class Payload {
        public FileNode sharedInfo;
        public bool showMask;
    }

    public GameObject operationMask;
    public Text textName;
    public Image logoImage;
    public Sprite[] logoSprite;
    private Payload payloadData;

    public FileNode ShareInfo {
        get { return payloadData.sharedInfo; }
    }

    public Type type;
    public string PathName;

    public enum Type {
        Project,
        Folder
    }

    public override void configureCellData()
    {
        payloadData = (Payload)DataObject;
        ShowOperationMask(payloadData.showMask);
        int index = payloadData.sharedInfo.PathName.LastIndexOf("/");
        string realName = payloadData.sharedInfo.PathName;
        if (index != -1)
        {
            realName = payloadData.sharedInfo.PathName.Substring(index + 1);
            PathName = payloadData.sharedInfo.PathName.Substring(0, index);
        }
        if (realName.StartsWith(CodeProjectRepository.FolderPrefix))
        {
           
            logoImage.sprite = logoSprite[1];
            textName.text = realName.Substring(CodeProjectRepository.FolderPrefix.Length);
            type = Type.Folder;
        }
        else {
            logoImage.sprite = logoSprite[0];
            textName.text = realName;
            type = Type.Project;
        }
        
    }

    public void ShowOperationMask(bool show) {
        operationMask.SetActive(show);
    }
}
