public class LobbySceneSaveState
{
    public enum Popup
    {
        Gameboard,
        Project,
    }

    public Popup initialPopup;
    public IRepositoryPath initialDir;
    public int initialGameboardThemeId; // only valid if initialPopup is Gameboard
}

public class LobbySceneController : SceneController
{
    public LobbyManager m_lobbyManager;

    public override void Init(object userData, bool isRestored)
    {
        base.Init(userData, isRestored);

        var state = (LobbySceneSaveState)userData;
        if (state != null)
        {
            m_lobbyManager.Restore(state);
        }
    }
}
