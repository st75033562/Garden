using DataAccess;
using UnityEngine.UI;

public class CompetitionProblemCellBase : ScrollableCell
{
    public AssetBundleSprite m_thumbnailSprite;
    public Text m_nameText;

    public override void ConfigureCellData()
    {
        if (problem.gameboardItem != null)
        {
            var templateData = GameboardTemplateData.Get(problem.gameboardItem.sceneId);
            m_thumbnailSprite.SetAsset(templateData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
        }
        else
        {
            m_thumbnailSprite.ShowDefaultSprite();
        }
        m_nameText.text = problem.name;
    }

    public CompetitionProblem problem
    {
        get { return (CompetitionProblem)dataObject; }
    }
}
