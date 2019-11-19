using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum CompetitionCategory
{
    Open,
    Closed,
    Mine, // for student, this is the joined competition
    OpenTest,
    ClosedTest,
    Num
}

public class CompetitionListModel : SimpleListItemModel<Competition>
{
    private readonly ICompetitionService m_service;
    private bool m_fetching;
    private Func<Competition, bool> m_filter;

    public CompetitionListModel(CompetitionCategory category, ICompetitionService service, Func<Competition, bool> filter = null)
        : base(new List<Competition>())
    {
        if (category == CompetitionCategory.Num)
        {
            throw new ArgumentOutOfRangeException("category");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }
        this.category = category;
        m_service = service;
        m_filter = filter;
    }

    public CompetitionCategory category { get; private set; }

    public bool fetched { get; private set; }

    public int indexOf(uint itemId)
    {
        return items.FindIndex(x => x.id == itemId);
    }

    public void fetch()
    {
        if (m_fetching)
        {
            return;
        }

        m_fetching = true;
        m_service.RetrieveCompetitions(category, result => {
            m_fetching = false;
            fetched = true;

            items.Clear();
            if (m_filter != null)
            {
                items.AddRange(result.Where(m_filter));
            }
            else
            {
                items.AddRange(result);
            }

            if (comparer != null)
            {
                sort();
            }
            fireReset();
        });
    }

    public bool hasCompetition(uint id)
    {
        return items.FindIndex(x => x.id == id) != -1;
    }
}
