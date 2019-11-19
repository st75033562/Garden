using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionStudentCell : CompetitionCellBase
{
    public GameObject m_joinedIndicator;
    public GameObject m_daysLeftGo;
    public Text m_daysLeftText;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();
        m_joinedIndicator.SetActive(competition.HasUserJoined(UserManager.Instance.UserId));

        if (m_daysLeftText)
        {
            var leftTime = competition.endTime - ServerTime.UtcNow;
            if (leftTime >= TimeSpan.Zero)
            {
                int leftDays = Mathf.CeilToInt((float)(leftTime.TotalMinutes / TimeUtils.MinutesPerDay));
                m_daysLeftText.text = "ui_pk_competition_left_days".Localize(leftDays);
            }
            m_daysLeftGo.SetActive(leftTime >= TimeSpan.Zero);
        }
    }
}
