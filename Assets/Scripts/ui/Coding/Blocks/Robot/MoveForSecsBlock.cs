using System.Collections;

public class MoveForSecsBlock : MoveForSecsBlockBase
{
    private DownMenuPlugins m_directionMenu;

    protected override void Start()
    {
        base.Start();

        m_directionMenu = GetComponentInChildren<DownMenuPlugins>();
    }

    protected override float speedMultiplier
    {
        get { return m_directionMenu.GetMenuValue() == "down_menu_move_forward" ? 1.0f : -1.0f; }
    }

    protected override float GetMoveTime(BlockState state)
    {
        float time;
        float.TryParse(state.slotValues[1], out time);
        return time;
    }
}
