using System;

public class ProjectViewPayload
{
    public bool showDeleteBtn;
    public bool showAddCell;
    public Action<IRepositoryPath> selectCallback;
    public IRepositoryPath initialDir;
}

public class PopupProjectView : PopupController
{
    public ProjectView view;
    private Action<IRepositoryPath> callback;

    protected override void Start()
    {
        base.Start();

        var projectViewPayload = (ProjectViewPayload)payload;
        callback = projectViewPayload.selectCallback;

        var config = new ProjectViewConfig {
            projectSelectCallback = OnSelectProject,
            closeCallback = Close,
            showSettingButton = false,
            showDeleteButton = projectViewPayload.showDeleteBtn,
            showAddCell = projectViewPayload.showAddCell,
            initialDir = projectViewPayload.initialDir
        };

        view.Initialize(config);
    }

    private void OnSelectProject(IRepositoryPath path)
    {
        Close();

        if (callback != null)
        {
            callback(path);
        }
    }
}
