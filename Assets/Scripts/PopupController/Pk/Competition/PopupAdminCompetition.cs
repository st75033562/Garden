using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupAdminCompetition : PopupCompetitionBase, ICompetitionView
{
    public Button m_homeButton;
    public Button m_addButton;
    public Button m_deleteButton;
    public Button m_testButton;
    public Button m_publishButton;
    public Button m_refreshButton;
    public ButtonColorEffect m_backButton;
    public GameObject m_emptyGo;

    public Toggle m_openToggle;
    public Toggle m_scratchToggle;
    public GameObject m_scratchView;

    public GameObject m_cancelButton;
    private PopupCompetitionProblems m_problemView;
    public GameObject publishPanel;
    public GameObject testPanel;
    public GameObject draftPanel;
    public Toggle publishOpenTog;
    public Toggle testOpenTog;

    private enum EditMode
    {
        None,
        Delete,
        Publish,
        Test
    }

    private EditMode m_mode = EditMode.None;
    
    protected override void Start()
    {
        base.Start();

        EventBus.Default.AddListener(EventId.CompetitionCreated, OnCompetitionCreated);
        EventBus.Default.AddListener(EventId.CompetitionUpdated, OnCompetitionUpdated);

        if (!UserManager.Instance.IsAdmin)
        {
            m_scratchToggle.gameObject.SetActive(false);
            m_scratchView.SetActive(false);
            m_openToggle.isOn = true;
            ShowOpenCompetitions();
        }
        else
        {
            m_openToggle.isOn = true;
            ShowOpenCompetitions();
        }
    }

    protected override CompetitionListModel CreateModel(CompetitionCategory category)
    {
        Func<Competition, bool> filter = null;
        if (category == CompetitionCategory.Mine)
        {
            filter = x => x.isScratch;
        }
        return new CompetitionListModel(category, m_service, filter);
    }

    protected override void OnCurrentViewPopulated()
    {
        UpdateUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventBus.Default.RemoveListener(EventId.CompetitionCreated, OnCompetitionCreated);
        EventBus.Default.RemoveListener(EventId.CompetitionUpdated, OnCompetitionUpdated);

        if (m_problemView)
        {
            PopupManager.Close(m_problemView);
        }
    }

    private void OnCompetitionCreated(object arg)
    {
        var item = (Competition)arg;
        m_models[(int)CompetitionCategory.Mine].addItem(item);
        UpdateUI();
        if (!m_problemView)
        {
            m_problemView = PopupManager.CompetitionProblems(item, m_service, true, () => m_problemView = null);
        }
    }

    private void OnCompetitionUpdated(object arg)
    {
        var item = (Competition)arg;
        var model = m_models[(int)CompetitionCategory.Mine];
        var index = model.indexOf(item);
        if (index != -1)
        {
            model.updatedItem(index);
        }
        else
        {
            Debug.LogError("cannot find competition with id: " + item.id);
        }
    }

    public void ShowScratchCompetitions()
    {
        NodeTemplateCache.Instance.ShowBlockUI = true;
        publishPanel.SetActive(false);
        testPanel.SetActive(false);
        draftPanel.SetActive(true);
        
        ShowCategory(CompetitionCategory.Mine);
    }

    public void PublishOpen() {
        NodeTemplateCache.Instance.ShowBlockUI = false;
        ShowCategory(CompetitionCategory.Open);
    }

    public void PublishClose() {
        NodeTemplateCache.Instance.ShowBlockUI = false;
        ShowCategory(CompetitionCategory.Closed);
    }

    public void TestOpen() {
        NodeTemplateCache.Instance.ShowBlockUI = false;
        ShowCategory(CompetitionCategory.OpenTest);
    }

    public void TestClose() {
        NodeTemplateCache.Instance.ShowBlockUI = false;
        ShowCategory(CompetitionCategory.ClosedTest);
    }

    public void ShowOpenCompetitions()  
    {
        publishPanel.SetActive(true);
        testPanel.SetActive(false);
        draftPanel.SetActive(false);
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
        publishPanel.SetActive(false);
        testPanel.SetActive(true);
        draftPanel.SetActive(false);
        if (testOpenTog.isOn) {
            TestOpen();
        }
        else {
            TestClose();
        }
        
    }

    protected override void ShowCategory(CompetitionCategory category)
    {
        base.ShowCategory(category);

        SetMode(EditMode.None);
    }

    public void OnClickAddCompetition()
    {
        PopupManager.EditCompetition(null, m_service);
    }

    public void BeginDelete()
    {
        SetMode(EditMode.Delete);
    }

    public void BeginPublish()
    {
        SetMode(EditMode.Publish);
    }

    public void BeginTest() {
        SetMode(EditMode.Test);
    }
    public void EndEdit()
    {
        SetMode(EditMode.None);
    }

    private void SetMode(EditMode mode)
    {
        m_mode = mode;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isAdmin = UserManager.Instance.IsAdmin;

        m_addButton.gameObject.SetActive(isAdmin && draftPanel.gameObject.activeSelf);
        m_addButton.interactable = !isEditing;

        m_backButton.interactable = !isEditing;
        m_homeButton.interactable = !isEditing;

        m_deleteButton.interactable = m_mode != EditMode.Publish && m_mode != EditMode.Test;
        m_deleteButton.gameObject.SetActive(isAdmin);

        m_testButton.interactable = m_mode != EditMode.Delete && m_mode != EditMode.Publish;
        m_testButton.gameObject.SetActive(currentCategory == CompetitionCategory.Mine && isAdmin);

        m_publishButton.interactable = m_mode != EditMode.Delete && m_mode != EditMode.Test;
        m_publishButton.gameObject.SetActive(currentCategory == CompetitionCategory.Mine && isAdmin);

        m_refreshButton.interactable = !isEditing;
        m_refreshButton.gameObject.SetActive(currentCategory != CompetitionCategory.Mine);

        currentScrollController.Refresh();

        m_cancelButton.SetActive(isEditing);
        m_emptyGo.SetActive(currentModel.count == 0 && !isEditing && isAdmin && 
                            currentCategory == CompetitionCategory.Mine);
    }

    public void OnClickCell(CompetitionCellBase cell)
    {
        switch (m_mode)
        {
        case EditMode.None:
            PopupManager.CompetitionProblems(cell.competition, 
                                             m_service, 
                                             cell.competition.category == CompetitionCategory.Mine);
            break;
        case EditMode.Delete:
            DeleteCompetition(cell.competition);
            break;
        case EditMode.Publish:
            PublishCompetition(cell.competition);
            break;
        case EditMode.Test:
            TestCompetition(cell.competition);
            break;
        }
    }

    private void DeleteCompetition(Competition competition)
    {
        PopupManager.YesNo("ui_pk_competition_delete_confirm".Localize(),
            () => {
                m_service.DeleteCompetition(competition.id, res => {
                    if (res == Command_Result.CmdNoError || res == Command_Result.CmdCourseNotFound)
                    {
                        RemoveCompetition(competition);
                    }
                    else
                    {
                        Debug.LogError(res);
                    }
                });
            });
    }

    private void RemoveCompetition(Competition competition)
    {
        var model = m_models[(int)competition.category];
        var index = model.indexOf(competition);
        if (index != -1)
        {
            model.removeItem(index);
        }

        if (model.count == 0 && isEditing)
        {
            EndEdit();
        }
    }

    private void PublishCompetition(Competition competition)
    {
        m_service.PublishCompetition(competition, res => {
            if (res == Command_Result.CmdNoError)
            {
                PopupManager.Notice("ui_pk_competition_publish_done".Localize());
                m_models[(int)CompetitionCategory.Open].fetch();
            }
            else
            {
                PopupManager.Notice("ui_pk_competition_publish_failed".Localize());
            }
        });
    }

    private void TestCompetition(Competition competition)
    {
        var passwordData = new SetPasswordData((str)=> {
            Debug.Log(str);
            m_service.TestCompetition(competition, str, res =>
            {

                if (res == Command_Result.CmdNoError)
                {
                    PopupManager.Notice("ui_pk_competition_publish_done".Localize());
                    m_models[(int)CompetitionCategory.OpenTest].fetch();
                }
                else
                {
                    PopupManager.Notice("ui_pk_competition_publish_failed".Localize());
                }
            });
        }, null);

        PopupManager.SetPassword("ui_text_to_test_section".Localize(), "ui_set_password".Localize(), passwordData);
    }

    public bool isEditing
    {
        get { return m_mode != EditMode.None; }
    }
}
