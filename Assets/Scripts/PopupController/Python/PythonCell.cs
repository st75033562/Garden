using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PythonData {
    public bool isDeleteState;
    public PathInfo pathInfo;
    public bool isProgramFolder; // only valid when programFolderOnly is true
}

public class PythonCell : ScrollCell {
    [SerializeField]
    private GameObject delGo;
    [SerializeField]
    private Button cellBtn;
    [SerializeField]
    private Image imageIcon;
    [SerializeField]
    private Sprite fileIcon;
    [SerializeField]
    private Sprite folderIcon;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private GameObject operationGo;

    private PythonData pythonData;

    public override void configureCellData() {
        pythonData = (PythonData)DataObject;

        operationGo.SetActive(IsSelected);

        cellBtn.interactable = !pythonData.isDeleteState;
        delGo.SetActive(pythonData.isDeleteState);

        if(pythonData.pathInfo.path.isDir)
        {
            imageIcon.sprite = folderIcon;
        }
        else
        {
            imageIcon.sprite = fileIcon;
        }

        nameText.text = pythonData.pathInfo.path.name;
    }

    public void OnClickAdd() {
        (Context as PopupPythonProjectView).ClickAdd();
    }

    public void OnClickDel() {
        PopupManager.YesNo("ui_confirm_delete".Localize(), ()=> {
            (Context as PopupPythonProjectView).ClickdelCell(pythonData);
        });
    }

    public void OnClickCell() {
        var parent = Context as PopupPythonProjectView;
        parent.ClickCell(pythonData);
        if (!parent.inSelectionMode && !pythonData.pathInfo.path.isDir)
        {
            IsSelected = !IsSelected;
        }
    }

    public void ShowDelUI(bool isShow) {
        if(isShow) {
            cellBtn.interactable = false;
            delGo.SetActive(true);
        } else {
            cellBtn.interactable = true;
            delGo.SetActive(false);
        }
    }

    public void OnClickEditor() {
        (Context as PopupPythonProjectView).OnClickEditor(pythonData);
    }

    public void OnClickPlay() {
        (Context as PopupPythonProjectView).OnClickPlay(pythonData);
    }
}
