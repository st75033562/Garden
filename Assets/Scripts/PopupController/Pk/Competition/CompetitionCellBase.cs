using UnityEngine;
using UnityEngine.UI;

public interface ICompetitionView
{
    bool isEditing { get; }
}

public class CompetitionCellBase : ScrollableCell
{
    public Text m_nameText;
    public Text m_startTimeText;
    public Text m_endTimeText;

    public Image m_selectionMask;
    public UIImageMedia m_coverImage;

    public override void ConfigureCellData()
    {
        m_nameText.text = competition.name;
        if (m_startTimeText)
        {
            m_startTimeText.text = competition.startTime.ToLocalTime().ToString("ui_pk_competition_date_format".Localize());
        }
        m_endTimeText.text = competition.endTime.ToLocalTime().ToString("ui_pk_competition_date_format".Localize());

        if (m_selectionMask)
        {
            var view = (ICompetitionView)controller.context;
            m_selectionMask.enabled = view.isEditing;
        }

        m_coverImage.SetImage(competition.coverUrl);
    }

    public Competition competition
    {
        get { return (Competition)dataObject; }
    }
}
