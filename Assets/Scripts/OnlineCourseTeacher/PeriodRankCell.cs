using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeriodRankCell : ScrollCell {
    public class Payload{
        public uint[] startScore;
        public Rank_Unit rankUnit;
    }
    public Image imageRank;
    public Text rankText;
    public Image avatarImage;
    public Sprite[] rankSprites;
    public Text userName;
    public GameObject[] stars;
    public Text textScore;
    public Color[] color;
    public override void configureCellData() {
        var data = (Payload)DataObject;
        imageRank.gameObject.SetActive(true);
        rankText.gameObject.SetActive(false);
        textScore.text = "ui_leaderboard_score".Localize() + "\n" + data.rankUnit.RankScore;
        if (DataIndex == 0) {
            imageRank.sprite = rankSprites[0];
            textScore.color = color[0];
        }
        else if (DataIndex == 1)
        {
            imageRank.sprite = rankSprites[1];
            textScore.color = color[1];
        }
        else if (DataIndex == 2)
        {
            imageRank.sprite = rankSprites[2];
            textScore.color = color[2];
        }
        else {
            imageRank.gameObject.SetActive(false);
            rankText.gameObject.SetActive(true);
            rankText.text =(DataIndex + 1).ToString();
            textScore.color = color[3];
        }
        
        avatarImage.sprite = UserIconResource.GetUserIcon((int)data.rankUnit.UserDisplayInfo.UserInconId);
        userName.text = data.rankUnit.UserDisplayInfo.UserNickname;
        int starCount = 0;
        if (data.rankUnit.RankScore >= data.startScore[2]) {
            starCount = 3;
        }
        else if (data.rankUnit.RankScore >= data.startScore[1])
        {
            starCount = 2;
        }
        else {
            starCount = 1;
        }
        for (int i =0; i< stars.Length; i++) {
            stars[i].SetActive(i < starCount);
        }
    }

}
