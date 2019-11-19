using UnityEngine;

public class PopupSinglePkLeaderboard : PopupController
{
    public ScrollableAreaController m_scroll;
    public SinglePkMyLeaderboardCell m_myLeaderboardCell;
    public GameObject contentBottom;
    private GameBoard m_gameBoard;
    private SinglePkLeaderboardListModel m_model;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        m_gameBoard = (GameBoard)payload;
        contentBottom.SetActive(UserManager.Instance.UserId != m_gameBoard.GbCreateId);
        var leaderboard = new RemoteSingleLeaderboardDataSource(m_gameBoard.GbId);
        //var leaderboard = new TestSingleLeaderboardDataSource(20, true);
        m_model = new SinglePkLeaderboardListModel(m_gameBoard, leaderboard);
        UpdateMyCell();

        m_scroll.InitializeWithData(m_model);
        m_model.onReset += UpdateMyCell;
        m_model.fetch();
    }

    private void UpdateMyCell()
    {
        m_myLeaderboardCell.DataObject = m_model.myItem;
    }

    public void OnClickChallenge() {
        var helper = new SinglePkChallengeHelper(m_gameBoard);
        helper.onUploaded += m_model.fetch;
        if((PopupILPeriod.PassModeType)m_gameBoard.Parameter.jsPassMode == PopupILPeriod.PassModeType.Submit) {
            helper.UploadAnswer();
        } else {
            helper.EvaluateAnswer(null, false, PopupILPeriod.PassModeType.Play);
        }
    }

    public void ShowDetails()
    {
        PopupManager.SinglePkDetail(m_gameBoard, false);
    }
}
