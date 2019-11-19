public class PopupCompetitionBase : PopupController
{
    public ScrollableAreaController[] m_scrollControllers;

    protected readonly ICompetitionService m_service = new RemoteCompetitionService();
    protected CompetitionListModel[] m_models;

    private CompetitionCategory m_currentCategory;

    protected override void Start()
    {
        base.Start();

        InitializeModels();
        InitializeOpenCompetitionMonitor();
    }

    private void InitializeModels()
    {
        m_models = new CompetitionListModel[(int)CompetitionCategory.Num];
        for (int i = 0; i < m_models.Length; ++i)
        {
            var category = (CompetitionCategory)i;
            m_models[i] = CreateModel(category);
            m_models[i].onReset += () => {
                if (category == currentCategory)
                {
                    OnCurrentViewPopulated();
                }
            };
            m_scrollControllers[i].InitializeWithData(m_models[i]);
            m_scrollControllers[i].context = this;
        }
    }

    protected virtual CompetitionListModel CreateModel(CompetitionCategory category)
    {
        return new CompetitionListModel(category, m_service);
    }

    protected virtual void OnCurrentViewPopulated()
    {
    }

    private void InitializeOpenCompetitionMonitor()
    {
        var openModel = m_models[(int)CompetitionCategory.Open];
        var closedModel = m_models[(int)CompetitionCategory.Closed];

        var openCompetitionMonitor = gameObject.AddComponent<OpenCompetitionMonitor>();
        openCompetitionMonitor.Initialize(openModel, closedModel);
    }

    protected virtual void ShowCategory(CompetitionCategory category)
    {
        int newIndex = (int)category;
        if (!m_models[newIndex].fetched)
        {
            m_models[newIndex].fetch();
        }
        for (int i = 0; i < (int)CompetitionCategory.Num; ++i)
        {
            m_scrollControllers[i].gameObject.SetActive(i == newIndex);
        }
        m_currentCategory = category;
    }

    protected CompetitionCategory currentCategory
    {
        get { return m_currentCategory; }
    }
    
    protected CompetitionListModel currentModel
    {
        get { return m_models[(int)m_currentCategory]; }
    }

    protected ScrollableAreaController currentScrollController
    {
        get { return m_scrollControllers[(int)m_currentCategory]; }
    }

    public void OnClickRefresh()
    {
        currentModel.fetch();
    }
}
