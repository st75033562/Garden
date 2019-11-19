using System.Collections;

public class ControlTimerBlock : BlockBehaviour
{
    private DownMenuPlugins m_menu;

    protected override void Start()
    {
        base.Start();
        m_menu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        switch (m_menu.GetMenuValue())
        {
        case "block_timer_start":
            CodeContext.timer.Start();
            break;
        case "block_timer_pause":
            CodeContext.timer.Pause();
            break;
        case "block_timer_reset":
            CodeContext.timer.Reset();
            break;
        }

        yield break;
    }
}
