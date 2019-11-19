using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HonorrankCell : ScrollCell {
    public Image avatar;
    public Sprite[] rankMarkSprite;
    public Image rankMark;
    public Text textRank;
    public Text userName;
    public Text certificateCount;
    public Text trophyCount;


    public override void configureCellData() {
        Parse((Honor_Rank_Unit)DataObject);
    }

    public void Parse(Honor_Rank_Unit rankUnit) {
        if(rankUnit == null) {
            avatar.sprite = UserIconResource.GetUserIcon(UserManager.Instance.AvatarID);
            userName.text = UserManager.Instance.Nickname;
            rankMark.gameObject.SetActive(false);
            textRank.gameObject.SetActive(true);
            textRank.text = "ui_pk_no_rank".Localize();
            return;
        }
        var userInfo = rankUnit.UserDisplayInfo;
        avatar.sprite = UserIconResource.GetUserIcon((int)userInfo.UserInconId);
        userName.text = userInfo.UserNickname;
        certificateCount.text = rankUnit.CertificateCount.ToString();
        trophyCount.text = rankUnit.TrophyCount.ToString();
        if(rankUnit.RankId == 0) {
            rankMark.gameObject.SetActive(false);
            textRank.gameObject.SetActive(true);
            textRank.text = "ui_pk_no_rank".Localize();
        } else if(rankUnit.RankId <= 3) {
            rankMark.gameObject.SetActive(true);
            textRank.gameObject.SetActive(false);
            rankMark.sprite = rankMarkSprite[rankUnit.RankId - 1];
        } else {
            rankMark.gameObject.SetActive(false);
            textRank.gameObject.SetActive(true);
            textRank.text = rankUnit.RankId.ToString();
        }
    }
}
