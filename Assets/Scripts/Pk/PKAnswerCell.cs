using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class PKAnswerCellData
{
    public bool canChallenge;
    public PKAnswer pkAnswer;
}

public class PKAnswerCell : ScrollCell
{
    [SerializeField]
    private Text textName;
    [SerializeField]
    private UILikeWidget likeWidget;

    [SerializeField]
    private GameObject challengeGo;

    public PKAnswer pkAnswer
    {
        get { return ((PKAnswerCellData)DataObject).pkAnswer; }
    }

    public override void configureCellData()
    {
        Cleanup();
        pkAnswer.onPKResultAdded += OnResultAdded;

        textName.text = pkAnswer.AnswerName;
        challengeGo.SetActive((DataObject as PKAnswerCellData).canChallenge);
        UpdateLikeWidget(false);
    }

    private void OnResultAdded(PK_Result result)
    {
        configureCellData();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (pkAnswer != null)
        {
            pkAnswer.onPKResultAdded -= OnResultAdded;
        }
    }

    private void UpdateLikeWidget(bool showAnim)
    {
        likeWidget.likeCount = pkAnswer.PkLikeList.Count;
        likeWidget.SetLiked(pkAnswer.Liked(UserManager.Instance.UserId), showAnim);
    }

    public void OnClickLike()
    {
        if (pkAnswer.PkLikeList.ContainsKey(UserManager.Instance.UserId))
        {
            pkAnswer.PkLikeList.Remove(UserManager.Instance.UserId);
        }
        else
        {
            pkAnswer.PkLikeList.Add(UserManager.Instance.UserId, new PKLike_Info());
        }
        UpdateLikeWidget(true);
        var pkLike = new CMD_Like_PK_r_Parameters();
        pkLike.PkId = (Context as PkAnswersView).pk.PkId;
        pkLike.PkAnswerId = pkAnswer.AnswerId;
        SocketManager.instance.send(Command_ID.CmdLikePkR, pkLike.ToByteString());
    }

}
