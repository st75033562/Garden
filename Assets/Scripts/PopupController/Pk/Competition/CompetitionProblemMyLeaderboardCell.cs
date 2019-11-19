using UnityEngine;

public class CompetitionProblemMyLeaderboardCell : CompetitionProblemLeaderboardCell
{
    public GameObject m_challengeGo;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        bool showChallenge = item.problem.gameboardItem != null && item.answer == null;
        showChallenge &= item.problem.competition.state == Competition.OpenState.Open;
        m_challengeGo.SetActive(showChallenge);
        m_previewGo.SetActive(!showChallenge && item.canPreview);
        m_scoreLabel.enabled = m_scoreText.enabled = !showChallenge;
    }
}
