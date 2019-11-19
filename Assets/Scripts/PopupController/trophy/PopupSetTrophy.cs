using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSetTrophyData {
    public Course_Race_Type courseRaceType;
    public CourseTrophySetting trophySetting;
}

public class PopupSetTrophy : PopupController {
    public Button btnNext;
    public Toggle first;
    public Toggle second;
    public GameObject routineGo;
    public GameObject rankGo;
    public InputField goldInput;
    public InputField silveInput;
    public InputField bronzeInput;
    public GameObject btnAddTrophy;
    public AssetBundleSprite[] imgBody;
    public AssetBundleSprite[] imgBase;
    public AssetBundleSprite[] imgHandle;
    public AssetBundleSprite[] imgPattern;
    public GameObject routinePanel;
    public GameObject rankPanel;
    public ScrollLoopController scroll;
    public Button btnOk;

    public PopupSetTrophyData setTrophyData;
    private TrophySetting currentTrophy;
    private SetTrophyTypeCell preTrophyCell;
    protected override void Start() {
        base.Start();
        setTrophyData = (PopupSetTrophyData)payload;

        if(setTrophyData.courseRaceType == Course_Race_Type.CrtRankedMatch) {
            rankGo.SetActive(true);
            routineGo.SetActive(false);
        } else {
            rankGo.SetActive(false);
            routineGo.SetActive(true);
            
        }
        btnOk.interactable = false;

        if (setTrophyData.trophySetting != null && setTrophyData.trophySetting.courseRaceType == setTrophyData.courseRaceType) {
            UpdateTrophyUI(setTrophyData.trophySetting.goldTrophy);

            goldInput.text = setTrophyData.trophySetting.goldTrophy.miniScore.ToString();
            silveInput.text = setTrophyData.trophySetting.silverTrophy.miniScore.ToString();
            bronzeInput.text = setTrophyData.trophySetting.bronzeTrophy.miniScore.ToString();

            btnOk.interactable = setTrophyData.trophySetting.goldTrophy.trophyResultId !=0;

            CheckFinish();
        }
    }

    public void OnClickNext() {
        second.isOn = true;
        routinePanel.SetActive(false);
        rankPanel.SetActive(true);
        scroll.context = this;
        scroll.initWithData(TrophyResultData.GetAllTrophyData());
    }

    public void OnClickTrophyStyle() {
        PopupManager.SelectTrophyType(title, (trophyPb)=> {
            UpdateTrophyUI(trophyPb);
            CheckFinish();
        });
    }

    void UpdateTrophyUI(TrophySetting trophy) {
        currentTrophy = trophy;

        UpdatePartUI(imgBody, trophy.bodyId);
        UpdatePartUI(imgBase, trophy.baseId);
        UpdatePartUI(imgHandle, trophy.handleId);
        UpdatePartUI(imgPattern, trophy.patternId);

        btnAddTrophy.SetActive(false);
    }

    private void UpdatePartUI(AssetBundleSprite[] images, int partId)
    {
        if (partId != 0) {
            var data = TrophyData.GetTrophyData(partId);
            images[0].SetAsset(data.assetBundleName, data.assetNameGold);
            images[1].SetAsset(data.assetBundleName, data.assetNameSilver);
            images[2].SetAsset(data.assetBundleName, data.assetNameBronze);
        }
    }

    public void InputEnd() {
        CheckFinish();
    }
    void CheckFinish() {
        if(setTrophyData.courseRaceType == Course_Race_Type.CrtRegularSeason) {
            if(string.IsNullOrEmpty(goldInput.text) || string.IsNullOrEmpty(silveInput.text) || string.IsNullOrEmpty(bronzeInput.text)) {
                btnNext.interactable = false;
                return;
            }
        }
        btnNext.interactable = currentTrophy != null;

    }

    public void OnClickTrophyResout(SetTrophyTypeCell cell) {
        currentTrophy.trophyResultId = cell.trophyResult.id;
        btnOk.interactable = currentTrophy.trophyResultId != 0;
    }

    public void OnClickOk() {
        if(setTrophyData.trophySetting == null) {
            setTrophyData.trophySetting = new CourseTrophySetting();
        }

        CourseTrophySetting courseTrophySetting = setTrophyData.trophySetting;
        courseTrophySetting.courseRaceType = setTrophyData.courseRaceType;
        courseTrophySetting.goldTrophy = currentTrophy;
        courseTrophySetting.silverTrophy = currentTrophy.Clone();
        courseTrophySetting.bronzeTrophy = currentTrophy.Clone();

        if(setTrophyData.courseRaceType == Course_Race_Type.CrtRegularSeason) {
            courseTrophySetting.goldTrophy.miniScore = int.Parse(goldInput.text);
            courseTrophySetting.silverTrophy.miniScore = int.Parse(silveInput.text);
            courseTrophySetting.bronzeTrophy.miniScore = int.Parse(bronzeInput.text);
        }

        Close();
    }

    public void OnClickPre() {
        routinePanel.SetActive(true);
        rankPanel.SetActive(false);
    }
}
