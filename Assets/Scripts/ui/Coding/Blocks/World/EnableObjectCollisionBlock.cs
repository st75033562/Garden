using System.Collections;

public class EnableObjectCollisionBlock : BlockBehaviour
{
    private EnableObjectCollisionMenuPlugins m_enableMenu;
    private DataMenuPlugins m_dataMenu;

    protected override void Start()
    {
        base.Start();

        m_enableMenu = GetComponentInChildren<EnableObjectCollisionMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
        m_dataMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        bool enabled = m_enableMenu.GetMenuValue() == "block_enable";
        var data = CodeContext.variableManager.get<VariableData>(m_dataMenu.GetMenuValue());
        if (data != null)
        {
            CodeContext.worldApi.SetObjectCollision((int)data.getValue(), enabled);
        }

        yield break;
    }
}
