using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompetitionProblems : PopupController, IProblemView
{
    public ScrollableAreaController m_scrollArea;
    public GameObject m_emptyStateGo;

    public Button m_newButton;
    public Button m_homeButton;
    public GameObject m_deleteGo;
    public ButtonColorEffect m_backButton;
    public Button m_editButton;
    public ButtonColorEffect m_editButtonEffect;
    public GameObject m_editImageGo;
    public GameObject m_cancelGo;

    public Text m_titleText;

    private Competition m_competition;
    private ICompetitionService m_service;
    private bool m_allowEditing;
    private bool m_isDeleting;

    public void Initialize(Competition competition, ICompetitionService service, bool allowEditing)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }

        m_competition = competition;
        m_service = service;
        m_allowEditing = allowEditing;
    }

    protected override void Start()
    {
        base.Start();

        EventBus.Default.AddListener(EventId.CompetitionUpdated, OnCompetitionUpdated);
        EventBus.Default.AddListener(EventId.CompetitionProblemUpdated, OnCompetitionProblemUpdated);

        m_scrollArea.context = this;
        m_scrollArea.InitializeWithData(new CompetitionProblemListModel(m_competition, false));

        m_competition.onProblemAdded += OnProblemAdded;
        m_competition.onProblemRemoved += OnProblemRemoved;

        UpdateUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_competition.onProblemAdded -= OnProblemAdded;
        m_competition.onProblemRemoved -= OnProblemRemoved;

        EventBus.Default.RemoveListener(EventId.CompetitionProblemUpdated, OnCompetitionProblemUpdated);
        EventBus.Default.RemoveListener(EventId.CompetitionUpdated, OnCompetitionUpdated);
    }

    private void OnProblemAdded(CompetitionProblem item)
    {
        UpdateUI();
    }

    private void OnProblemRemoved(CompetitionProblem item)
    {
        if (m_scrollArea.model.count == 0 && m_isDeleting)
        {
            m_isDeleting = false;
        }
        UpdateUI();
    }

    private void OnCompetitionUpdated(object arg)
    {
        UpdateUI();
    }

    private void OnCompetitionProblemUpdated(object arg)
    {
        m_scrollArea.model.updatedItem(arg);
    }

    private void UpdateUI()
    {
        bool empty = m_scrollArea.model.count == 0;
        m_scrollArea.gameObject.SetActive(!empty);
        m_emptyStateGo.SetActive(empty && !isEditing);
        m_titleText.text = m_competition.name;

        m_homeButton.interactable = !isEditing;

        m_deleteGo.SetActive(m_allowEditing);

        m_newButton.gameObject.SetActive(m_allowEditing);
        m_newButton.interactable = !isEditing;

        m_backButton.interactable = !isEditing;

        m_editImageGo.SetActive(m_allowEditing);
        m_editButton.enabled = m_allowEditing;
        m_editButtonEffect.interactable = !isEditing;

        m_cancelGo.SetActive(isEditing);

        m_scrollArea.Refresh();
    }

    public void OnClickEditCompetition()
    {
        PopupManager.EditCompetition(m_competition, m_service);
    }

    public void OnClickAddProblem()
    {
        PopupManager.EditCompetitionProblem(m_competition, null, m_service);
    }

    public void BeginDelete()
    {
        m_isDeleting = true;
        UpdateUI();
    }

    public void EndDelete()
    {
        m_isDeleting = false;
        UpdateUI();
    }

    public void OnClickProblem(CompetitionProblemCellBase cell)
    {
        if (m_isDeleting)
        {
            var problem = cell.problem;
            PopupManager.YesNo("ui_pk_competition_problem_delete_confirm".Localize(), 
                                  () => DeleteProblem(problem));
        }
        else if (m_allowEditing)
        {
            PopupManager.EditCompetitionProblem(m_competition, cell.problem, m_service);
        }
        else
        {
            PopupManager.CompetitionProblemLeaderboard(cell.problem, m_service, !UserManager.Instance.IsAdmin);
          //  PopupManager.CompetitionProblemDetail(cell.problem, m_service, false);
        }
    }

    private void DeleteProblem(CompetitionProblem problem)
    {
        m_service.DeleteProblem(problem, res => {
            if (res != Command_Result.CmdNoError)
            {
                PopupManager.Notice("ui_pk_competition_problem_delete_failed".Localize());
            }
        });
    }

    public bool isEditing
    {
        get { return m_isDeleting; }
    }
}
