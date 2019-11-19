using UnityEngine;
using UnityEngine.UI;

public class CompetitionProblemStudentCell : CompetitionProblemCellBase
{
    public Text m_answerCountText;
    public GameObject m_scoreGo;
    public Text m_myScoreText;
    public GameObject m_challengeGo;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        m_answerCountText.text = problem.answerCount.ToString();

        int score = problem.GetScore(UserManager.Instance.UserId);
        if (problem.competition.state == Competition.OpenState.Closed)
        {
            m_scoreGo.SetActive(true);
            if (score >= 0)
            {
                m_myScoreText.text = "ui_pk_competition_problem_score".Localize(score);
            }
            else
            {
                m_myScoreText.text = "ui_pk_competition_problem_did_not_join".Localize();
            }
        }
        else if (score >= 0)
        {
            m_scoreGo.SetActive(true);
            m_myScoreText.text = "ui_pk_competition_problem_score".Localize(score);
        }
        else
        {
            m_scoreGo.SetActive(false);
        }

        m_challengeGo.SetActive(score < 0 && problem.competition.state == Competition.OpenState.Open);
    }
}
