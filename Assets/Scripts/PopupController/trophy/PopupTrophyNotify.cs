using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupTrophyNotify : PopupController {

    public Text textCourseName;
    public AssetBundleSprite imgBody;
    public AssetBundleSprite imgBase;
    public AssetBundleSprite imgHandle;
    public AssetBundleSprite imgPattern;

    protected override void Start() {
        base.Start();

        textCourseName.text = userTrophy.trophyPb.courseName;

        UpdateImage(imgBody, userTrophy.awardTrophy, userTrophy.trophyPb.bodyId);
        UpdateImage(imgBase, userTrophy.awardTrophy, userTrophy.trophyPb.baseId);
        UpdateImage(imgHandle, userTrophy.awardTrophy, userTrophy.trophyPb.handleId);
        UpdateImage(imgPattern, userTrophy.awardTrophy, userTrophy.trophyPb.patternId);
    }

    private void UpdateImage(AssetBundleSprite sprite, Trophy_Type type, int partId)
    {
        if (partId != 0) {
            var data = TrophyData.GetTrophyData(partId);
            if(type == Trophy_Type.TtGold) {
                sprite.SetAsset(data.assetBundleName, data.assetNameGold);
            } else if(type == Trophy_Type.TtSilver) {
                sprite.SetAsset(data.assetBundleName, data.assetNameSilver);
            } else {
                sprite.SetAsset(data.assetBundleName, data.assetNameBronze);
            }
        }
    }

    private UserTrophy userTrophy
    {
        get { return (UserTrophy)payload; }
    }

    public void OnClickConfirm() {
        int popupId = PopupManager.ShowMask();
        var setHonorState = new CMD_Update_Honorwall_State_r_Parameters();
        setHonorState.CourseId = userTrophy.courseId;
        setHonorState.Trophy = 1;
        SocketManager.instance.send(Command_ID.CmdUpdateHonorwallStateR, setHonorState.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res != Command_Result.CmdNoError) {
                PopupManager.Notice(res.Localize());
            }
            OnCloseButton();
        });
    }
}
