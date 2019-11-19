using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPeriodRank : PopupController {
    public ScrollLoopController scroll;
    public class PayLoad {
        public uint courseId;
        public uint periodId;
        public uint[] startScore;
    }
    // Use this for initialization
    protected override void Start () {
        var data = (PayLoad)payload;
        var rankR = new CMD_Get_Ranklist_r_Parameters();
        rankR.PeriodId = data.periodId;
        rankR.CourseId = data.courseId;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetRanklistR, rankR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if (res == Command_Result.CmdNoError)
            {
                CMD_Get_Ranklist_a_Parameters rankA = CMD_Get_Ranklist_a_Parameters.Parser.ParseFrom(content);
                List<PeriodRankCell.Payload> rankLists = new List<PeriodRankCell.Payload>();
                foreach (var rank in rankA.RankList) {
                    var payLoad = new PeriodRankCell.Payload();
                    payLoad.startScore = data.startScore;
                    payLoad.rankUnit = rank;
                    rankLists.Add(payLoad);
                }
                scroll.initWithData(rankLists);
                Debug.Log("===>"+rankA.RankList.Count);
            }
            else
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
