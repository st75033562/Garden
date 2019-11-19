using UnityEngine;
using UnityEngine.UI;

public interface IProblemView
{
    bool isEditing { get; }
}

public class CompetitionProblemEditorCell : CompetitionProblemCellBase
{
    public Image m_selectionMask;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        var view = (IProblemView)controller.context;
        m_selectionMask.enabled = view.isEditing;
    }
}
