using Google.Protobuf;
using System;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class SaveCodeController : SaveCodeControllerBase
{
    private UIWorkspace m_Workspace;

    public SaveCodeController(UIWorkspace workspace)
    {
        m_Workspace = workspace;
    }

    protected CodePanel CodePanel
    {
        get { return m_Workspace.CodePanel; }
    }

    public override bool isChanged
    {
        get { return m_Workspace.IsChanged; }
    }

    protected override string currentProjectName
    {
        get { return m_Workspace.ProjectName; }
    }

    protected override IDialogInputValidator CreateProjectNameValidator()
    {
        return new ProjectNameValidator(m_Workspace.WorkingDirectory, currentProjectName, CodeProjectRepository.instance);
    }

    protected override void SaveAndSynchro(string name, Action onSaved, Action onSaveError)
    {
        var project = m_Workspace.GetProject();
        project.name = name;

        var projectPath = m_Workspace.WorkingDirectory + name;

        var request = Uploads.UploadProjectV3(project, projectPath);
        request.Success(() => {
            CodeProjectRepository.instance.save("", request.files.FileList_);

            m_Workspace.ProjectName = name;
            m_Workspace.IsChanged = false;

            if (onSaved != null)
            {
                onSaved();
            }
        })
        .Error(onSaveError)
        .Execute();
    }
}
