using System;
using System.Collections.Generic;

public class CompetitionProblemListModel : SimpleListItemModel<CompetitionProblem>
{
    public CompetitionProblemListModel(Competition competition, bool hasNewCell)
        : base(new List<CompetitionProblem>())
    {
        if (competition == null)
        {
            throw new ArgumentNullException();
        }

        // for new button
        if (hasNewCell)
        {
            items.Add(null);
        }
        items.AddRange(competition.problems);
        competition.onProblemAdded += OnProblemAdded;
        competition.onProblemRemoved += OnProblemRemoved;
    }

    private void OnProblemRemoved(CompetitionProblem item)
    {
        removeItem(indexOf(item));
    }

    private void OnProblemAdded(CompetitionProblem item)
    {
        addItem(item);
    }
}
