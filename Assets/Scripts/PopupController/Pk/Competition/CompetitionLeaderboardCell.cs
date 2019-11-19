using UnityEngine.UI;

public class CompetitionLeaderboarCellData
{
    public CompetitionLeaderboardRecord rankRecord;
    public int totalProblemCount;
}

public class CompetitionLeaderboardCell : PkLeaderboardCell
{
    public Text m_problemCountText;

    public override void ConfigureCellData()
    {
        var cellData = (CompetitionLeaderboarCellData)DataObject;
        base.Configure(cellData.rankRecord);

        m_problemCountText.text = string.Format("{0}/{1}", 
            cellData.rankRecord.answeredProblemCount, cellData.totalProblemCount);
    }
}
