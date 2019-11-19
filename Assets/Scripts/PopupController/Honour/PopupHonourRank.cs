using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupHonourRank : PopupController {
    public ScrollLoopController scroll;
    public HonorrankCell honorRankCell;

    private BossState bossState = BossState.All;
    private CategoryState categoryState = CategoryState.Certificate;
    enum BossState {
        All = 4,
        Class = 2
    }

    enum CategoryState {
        Trophy,
        Certificate
    }
    protected override void Start () {
        base.Start();
        Refesh();
    }

    public void OnClickAll() {
        bossState = BossState.All;
        Refesh();
    }

    public void OnClickClass() {
        bossState = BossState.Class;
        Refesh();
    }

    public void OnClickCertificate() {
        categoryState = CategoryState.Certificate;
        Refesh();
    }

    public void OnClickTrophy() {
        categoryState = CategoryState.Trophy;
        Refesh();
    }

    void Refesh() {
        int popupId = PopupManager.ShowMask();
        var honorRankR = new CMD_Get_Honorwall_Rank_r_Parameters();
        honorRankR.ReqType = (uint)bossState + (uint)categoryState;
        SocketManager.instance.send(Command_ID.CmdGetHonorwallRankR, honorRankR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            var honorRankA = CMD_Get_Honorwall_Rank_a_Parameters.Parser.ParseFrom(content);
            scroll.initWithData(honorRankA.RankList);
            honorRankCell.Parse(honorRankA.SelfInfo);
        });
    }
}
