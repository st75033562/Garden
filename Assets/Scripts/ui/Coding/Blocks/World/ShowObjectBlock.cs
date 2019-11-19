using System.Collections;

public class ShowObjectBlock : BlockBehaviour
{
    private ShowObjectMenuPlugins m_showHideMenu;
    private DataMenuPlugins m_dataMenu;

    protected override void Start()
    {
        base.Start();

        m_showHideMenu = GetComponentInChildren<ShowObjectMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
        m_dataMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        bool visible = m_showHideMenu.GetMenuValue() == "block_show_object";
        var data = CodeContext.variableManager.get<VariableData>(m_dataMenu.GetMenuValue());
        if (data != null)
        {
            CodeContext.worldApi.ShowObject((int)data.getValue(), visible);
        }
        yield break;
    }
}
