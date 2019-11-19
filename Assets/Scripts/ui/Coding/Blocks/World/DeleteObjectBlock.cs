using System.Collections;

public class DeleteObjectBlock : BlockBehaviour
{
    private DataMenuPlugins m_dataMenu;

    protected override void Start()
    {
        base.Start();

        m_dataMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var data = CodeContext.variableManager.get<VariableData>(m_dataMenu.GetMenuValue());
        if (data != null)
        {
            CodeContext.worldApi.DeleteObject((int)data.getValue());
        }
        yield break;
    }
}
