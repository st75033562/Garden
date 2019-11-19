using DataAccess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoardCell : ScrollCell {
    [SerializeField]
    private Text textName;
    [SerializeField]
    private AssetBundleSprite assetBundleSprite;
    public Image imageIcon;
    public GameObject markGo;
    public GameObject selectMark;
    
    public GameBoardData gameBoardData;
	
    public override void configureCellData() {
        gameBoardData = (GameBoardData)DataObject;
        if(gameBoardData == null)
            return;

        selectMark.SetActive(false);
        if (gameBoardData.pathInfo.path.isDir) {
            textName.text = gameBoardData.pathInfo.path.name;
            imageIcon.gameObject.SetActive(true);
            assetBundleSprite.gameObject.SetActive(false);
        } else {
            assetBundleSprite.gameObject.SetActive(true);
            textName.text = gameBoardData.gameBoard.name;
            imageIcon.gameObject.SetActive(false);
            GameboardTemplateData gbThemeData = GameboardTemplateData.Get((int)gameBoardData.gameBoard.themeId);
            assetBundleSprite.SetAsset(gbThemeData.thumbnailBundleName, GameboardTemplateData.Thumbnail);         
        }
        UpdateMark();
        SetSelectMark(gameBoardData.showSelectMask);
    }

    public void OnClickCell() {
        if (gameBoardData.operationType == GameBoardOperationType.NONE) {
            ((PopupGameBoard)Context).OnClickCell(this);
        } else if (gameBoardData.operationType == GameBoardOperationType.DELETE) {
            OnClickDelete();
        } else if (gameBoardData.operationType == GameBoardOperationType.PUBLISH) {
            OnClickUpload();
        } else if (gameBoardData.operationType == GameBoardOperationType.PUBLISH_BANK) {
            if (gameBoardData.pathInfo.path.isDir) {
                ((PopupGameBoard)Context).OnClickCell(this);
            }
            else {
                SetSelectMark(((PopupGameBoard)Context).addOrRemoveCell(this));
            }
            
        }
    }

    public void SetSelectMark(bool active) {
        selectMark.SetActive(active);
    }
    void OnClickUpload() {
        ((PopupGameBoard)Context).OnClickShare(this);
    }

    void OnClickDelete() {
        PopupManager.YesNo("ui_confirm_delete".Localize(), () => {
            ((PopupGameBoard)Context).OnClickDelete(this);
        }, null);
    }

    public void UpdateMark() {
        markGo.SetActive(gameBoardData.operationType != GameBoardOperationType.NONE);
    }
}
