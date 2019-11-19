using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class PopupSelectTrophyType : PopupController {
    [Serializable]
    public class LeftClassifyCell {
        public Button btn;
        public GameObject goSign;
        public ScrollLoopController scroll;

        public void disableBtn(bool state) {
            btn.interactable = !state;
            goSign.SetActive(state);
            scroll.gameObject.SetActive(state);
        }
    }

    public LeftClassifyCell[] leftClassifyCells;
    public AssetBundleSprite imgBody;
    public AssetBundleSprite imgBase;
    public AssetBundleSprite imgHandle;
    public AssetBundleSprite imgPattern;
    public Button btnConfirm;

    private LeftClassifyCell preLeftClassifyCell;
    private List<TrophyData> prophyData;
    private TrophySetting trophySetting = new TrophySetting();
    private Action<TrophySetting> callBack;

    protected override void Start () {
        base.Start();
        prophyData = TrophyData.GetAllTrophyData();
        OnClickBody();
        imgBody.ShowDefaultSprite();
        imgBase.ShowDefaultSprite();
        imgHandle.ShowDefaultSprite();
        imgPattern.ShowDefaultSprite();
        callBack = (Action<TrophySetting>)payload;
    }

    public void OnClickBody() {
        var scroll = ResetPreCell(leftClassifyCells[0]);
        scroll.initWithData(prophyData.FindAll((x) => { return (TrophyData.Type)x.type == TrophyData.Type.Body; }));
    }

    public void OnClickHandle() {
        var scroll = ResetPreCell(leftClassifyCells[1]);
        scroll.initWithData(prophyData.FindAll((x) => { return (TrophyData.Type)x.type == TrophyData.Type.Handle; }));
    }

    public void OnClickBase() {
        var scroll = ResetPreCell(leftClassifyCells[2]);
        scroll.initWithData(prophyData.FindAll((x) => { return (TrophyData.Type)x.type == TrophyData.Type.Bottom; }));
    }

    public void OnClickPattern() {
        var scroll = ResetPreCell(leftClassifyCells[3]);
        scroll.initWithData(prophyData.FindAll((x) => { return (TrophyData.Type)x.type == TrophyData.Type.Decorate; }));
    }

    ScrollLoopController ResetPreCell(LeftClassifyCell currentCell) {
        if(preLeftClassifyCell != null)
            preLeftClassifyCell.disableBtn(false);
        currentCell.disableBtn(true);
        preLeftClassifyCell = currentCell;
        return currentCell.scroll;
    }
    
    public void OnClickCell(ProphyPartCell cell) {
        if((TrophyData.Type)cell.trophyData.type == TrophyData.Type.Body) {
            trophySetting.bodyId = cell.trophyData.id;
            imgBody.SetAsset(cell.trophyData.assetBundleName, cell.trophyData.assetNameGold);
        } else if((TrophyData.Type)cell.trophyData.type == TrophyData.Type.Handle) {
            trophySetting.handleId = cell.trophyData.id;
            imgHandle.SetAsset(cell.trophyData.assetBundleName, cell.trophyData.assetNameGold);
        } else if((TrophyData.Type)cell.trophyData.type == TrophyData.Type.Bottom) {
            trophySetting.baseId = cell.trophyData.id;
            imgBase.SetAsset(cell.trophyData.assetBundleName, cell.trophyData.assetNameGold);
        } else if((TrophyData.Type)cell.trophyData.type == TrophyData.Type.Decorate) {
            trophySetting.patternId = cell.trophyData.id;
            imgPattern.SetAsset(cell.trophyData.assetBundleName, cell.trophyData.assetNameGold);
        }
        if(trophySetting.bodyId != 0 && trophySetting.baseId != 0) {
            btnConfirm.interactable = true;
        }
    }

    public void OnClickClose() {
        callBack(trophySetting);
        Close();
    }

}