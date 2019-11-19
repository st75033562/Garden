using System.Collections;

public class GetScreenSizeBlock : BlockBehaviour
{
    private DownMenuPlugins m_menu;


    protected override void Start()
    {
        base.Start();
        m_menu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        switch (m_menu.GetMenuValue())
        {
        case "down_menu_screen_width":
            retValue.value = CodeContext.gameboardService.screenSize[0].ToString();
            break;

        case "down_menu_screen_height":
            retValue.value = CodeContext.gameboardService.screenSize[1].ToString();
            break;
        }

        yield break;
    }
}
