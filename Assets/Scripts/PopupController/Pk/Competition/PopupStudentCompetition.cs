using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupStudentCompetition : PopupCompetitionBase
{
    public UISortMenuWidget m_sortMenu;
    public Toggle m_mineToggle;
    public GameObject publishPanel;
    public GameObject testPanel;
    public GameObject draftPanel;
    public Toggle publishOpenTog;
    public Toggle testOpenTog;
    public GameObject btnAdd;

    private enum SortKey
    {
        StartTime,
        EndTime,
        Name
    }

    private static readonly string[] s_sortOptions = {
        "ui_pk_competition_sort_start_time",
        "ui_pk_competition_sort_end_time",
        "ui_pk_competition_sort_name",
    };

    private class SortCriterion
    {
        public SortKey sortKey = SortKey.StartTime;
        public bool asc = true;
    }

    private readonly SortCriterion[] m_sortCriterions = new SortCriterion[(int)CompetitionCategory.Num];

    private static IComparer GetComparer(SortCriterion crit)
    {
        Comparison<Competition> comp = null;
        switch (crit.sortKey)
        {
            case SortKey.StartTime:
                comp = (lhs, rhs) => lhs.startTime.CompareTo(rhs.startTime);
                break;

            case SortKey.EndTime:
                comp = (lhs, rhs) => lhs.endTime.CompareTo(rhs.endTime);
                break;

            case SortKey.Name:
                comp = (lhs, rhs) => lhs.name.CompareWithUICulture(rhs.name);
                break;

            default:
                throw new ArgumentException();
        }

        return DelegatedComparer.Of<Competition>(comp.Invert(!crit.asc));
    }

    protected override void Start()
    {
        InitializeSorting();

        base.Start();

        publishOpenTog.isOn = true;
        ShowOpenCompetitions();

        EventBus.Default.AddListener(EventId.CompetitionProblemUpdated, OnProblemUpdated);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventBus.Default.RemoveListener(EventId.CompetitionProblemUpdated, OnProblemUpdated);
    }

    protected override CompetitionListModel CreateModel(CompetitionCategory category)
    {
        var model = base.CreateModel(category);
        model.setSorter(GetComparer(m_sortCriterions[(int)category]));
        return model;
    }

    private void OnProblemUpdated(object obj)
    {
        var problem = (CompetitionProblem)obj;
        m_models[(int)problem.competition.category].updatedItem(problem.competition);

        // update the copy in another category
        if (problem.competition.category == CompetitionCategory.Open)
        {
            Synchronize(CompetitionCategory.Mine, problem, true);
        }
        else if (problem.competition.category == CompetitionCategory.Mine)
        {
            Synchronize(CompetitionCategory.Open, problem, false);
        }
    }

    private void Synchronize(CompetitionCategory category, CompetitionProblem problem, bool addIfNotFound)
    {
        var model = m_models[(int)category];
        var index = model.indexOf(problem.competition.id);
        if (index != -1)
        {
            var targetComp = (Competition)model.getItem(index);
            var targetProblem = targetComp.GetProblem(problem.id);
            if (targetProblem == null)
            {
                targetComp.AddProblem(problem.Clone());
            }
            else
            {
                targetProblem.AddOrUpdateAnswer(problem.GetAnswer(UserManager.Instance.UserId).Clone());
            }
            model.updatedItem(index);
        }
        else if (addIfNotFound)
        {
            var clone = problem.competition.Clone();
            clone.category = category;
            model.addItem(clone);
        }
    }

    private void InitializeSorting()
    {
        for (int i = 0; i < m_sortCriterions.Length; ++i)
        {
            m_sortCriterions[i] = new SortCriterion();
        }
        m_sortMenu.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        m_sortMenu.onSortChanged.AddListener(OnSortChanged);
    }

    public void ShowMineCompetitions()
    {
        btnAdd.SetActive(false);
        publishPanel.SetActive(false);
        testPanel.SetActive(false);
        draftPanel.SetActive(true);
        ShowCategory(CompetitionCategory.Mine);
    }

    public void ShowOpenCompetitions()
    {
        btnAdd.SetActive(false);
        publishPanel.SetActive(true);
        testPanel.SetActive(false);
        draftPanel.SetActive(false);
        ShowCategory(CompetitionCategory.Open);
        if (publishOpenTog.isOn)
        {
            PublishOpen();
        }
        else
        {
            PublishClose();
        }
    }

    public void ShowClosedCompetitions()
    {
        btnAdd.SetActive(true);
        publishPanel.SetActive(false);
        testPanel.SetActive(true);
        draftPanel.SetActive(false);
        ShowCategory(CompetitionCategory.Closed);
        if (testOpenTog.isOn)
        {
            TestOpen();
        }
        else
        {
            TestClose();
        }
    }

    protected override void ShowCategory(CompetitionCategory category)
    {
        base.ShowCategory(category);

        var crit = m_sortCriterions[(int)category];
        m_sortMenu.SetCurrentSort((int)crit.sortKey, crit.asc);
    }

    public void OnClickCell(CompetitionCellBase cell)
    {
        PopupManager.StudentCompetitionProblems(cell.competition, m_service);
    }

    private void OnSortChanged()
    {
        var crit = m_sortCriterions[(int)currentCategory];
        crit.sortKey = (SortKey)m_sortMenu.activeSortOption;
        crit.asc = m_sortMenu.sortAsc;

        currentModel.setSorter(GetComparer(crit));
    }

    public void PublishOpen()
    {
        ShowCategory(CompetitionCategory.Open);
    }

    public void PublishClose()
    {
        ShowCategory(CompetitionCategory.Closed);
    }

    public void TestOpen()
    {
        ShowCategory(CompetitionCategory.OpenTest);
    }

    public void TestClose()
    {
        ShowCategory(CompetitionCategory.ClosedTest);
    }

    public void OnClickAdd() {
        PopupManager.PopupAddMatchCourse(currentModel, ()=> {
            OnClickRefresh();
        });
    }
}
