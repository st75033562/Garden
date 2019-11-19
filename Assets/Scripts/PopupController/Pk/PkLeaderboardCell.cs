using UnityEngine.UI;

public class PkLeaderboardCell : ScrollableCell
{
    public Image m_avatarIcon;
    public Image m_rankImage;
    public Text m_rankText;
    public Text m_usernameText;
    public Text m_scoreLabel;
    public Text m_scoreText;

    public RankConfig m_rankConfig;

    public void Configure(LeaderboardRecord rankData)
    {
        m_avatarIcon.sprite = UserIconResource.GetUserIcon(rankData.iconId);

        bool hasRank = rankData.rank > 0;
        if (hasRank)
        {
            var rankSprite = m_rankConfig.GetSprite(rankData.rank);
            m_rankImage.sprite = rankSprite;
            m_rankImage.enabled = rankSprite != null;
            m_rankText.enabled = rankSprite == null;
            m_rankText.text = rankData.rank.ToString();
        }
        else
        {
            m_rankImage.enabled = false;
            m_rankText.enabled = true;
            m_rankText.text = "ui_pk_no_rank".Localize();
        }

        var color = m_rankConfig.GetColor(rankData.rank);
        m_scoreLabel.color = color;
        m_scoreText.color = color;
        m_scoreText.text = rankData.score.ToString();
        m_usernameText.text = rankData.username;
    }
}
