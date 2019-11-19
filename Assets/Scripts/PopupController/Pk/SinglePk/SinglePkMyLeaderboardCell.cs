using UnityEngine;

public class SinglePkMyLeaderboardCell : SinglePkLeaderboardCell
{
    public GameObject m_challengeGo;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        var item = (SinglePkLeaderboardItem)dataObject;
        bool showChallengeButton = item.rankRecord.rank == 0;
        m_challengeGo.SetActive(showChallengeButton);
        m_scoreText.enabled = m_scoreLabel.enabled = !showChallengeButton;
    }
}
