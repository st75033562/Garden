using System;

public class WorkspaceSettingsViewModel : ISystemSettingsDialogViewMode
{
    private readonly BlockLevel m_lastLevel;

    public WorkspaceSettingsViewModel(UIWorkspace workspace)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }
        this.Workspace = workspace;
        m_lastLevel = workspace.BlockLevel;
    }

    public UIWorkspace Workspace { get; private set; }

    public bool LevelWasChanged { get { return m_lastLevel != BlockLevel; } }

    public AutoStop StopMode
    {
        get { return Workspace.StopMode; }
        set { Workspace.StopMode = value; }
    }

    public BlockLevel BlockLevel
    {
        get { return Workspace.BlockLevel; }
        set { Workspace.BlockLevel = value; }
    }

    public bool SoundEnabled
    {
        get { return Workspace.SoundEnabled; }
        set { Workspace.SoundEnabled = value; }
    }

    public void OnClosed()
    {
		if (LevelWasChanged)
		{
            Workspace.RefreshFilterAndTemplateList();
		}
    }

    public bool StopModeVisible
    {
        get { return true; }
    }
}
