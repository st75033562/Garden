using System;
using System.Linq;
using UnityEngine;
using Google.Protobuf;

public class PkChallengeHelper
{
    private PkAnswerSelectView selectView;
    private readonly PK pk;
    private readonly PKAnswer rivalAnswer;

    public PkChallengeHelper(PkAnswerSelectView selectView, PK pk, PKAnswer rivalAnswer)
    {
        if (selectView == null)
        {
            throw new ArgumentNullException("view");
        }
        if (pk == null)
        {
            throw new ArgumentNullException("pk");
        }
        if (rivalAnswer == null)
        {
            throw new ArgumentNullException("rivalAnswer");
        }
        this.selectView = selectView;
        this.pk = pk;
        this.rivalAnswer = rivalAnswer;
    }

    public void Challenge()
    {
        selectView.gameObject.SetActive(true);
        var myAnswers = pk.GetUserAnswers(UserManager.Instance.UserId).ToList();
        selectView.SetData(myAnswers, (myAnswer) => {
            selectView.GetComponentInParent<Canvas>().enabled = false;

            int popupId = 0;
            popupId = PopupManager.GameboardPlayer(
                ProjectPath.Remote(pk.ProjPath),
                new[] {
                    myAnswer.ToRobotCodeInfo(),
                    rivalAnswer.ToRobotCodeInfo(),
                },
                (mode, result) => {
                    Debug.LogFormat("my: {0}, rival: {1}", result.robotScores[0], result.robotScores[1]);
                    UploadChallengeResult(myAnswer, rivalAnswer, result);
                    PopupManager.Close(popupId);
                },
                onClose: () => {
                    selectView.GetComponentInParent<Canvas>().enabled = true;
                });
        });
    }

    void UploadChallengeResult(PKAnswer myAnswer, PKAnswer rivalAnswer, Gameboard.GameboardResult result)
    {
        var pkParameters = new CMD_PK_r_Parameters();
        pkParameters.PkId = pk.PkId;

        PK_Result pkResult = new PK_Result();
        pkResult.ChanllengerAnswerId = myAnswer.AnswerId;
        pkResult.ChanllengerScore = result.robotScores[0];

        pkResult.AccepterId = rivalAnswer.AnswerId;
        pkResult.AccepterScore = result.robotScores[1];
        pkParameters.PkResultInfo = pkResult;

        SocketManager.instance.send(Command_ID.CmdPkR, pkParameters.ToByteString(), (res, content) => {
            if (res == Command_Result.CmdNoError)
            {
                var pk_a = CMD_PK_a_Parameters.Parser.ParseFrom(content);
                myAnswer.AddPKResult(pk_a.PkResultInfo);
                rivalAnswer.AddPKResult(pk_a.PkResultInfo);
            }
            else
            {
                Debug.LogError("" + res);
            }
        });
    }
}
