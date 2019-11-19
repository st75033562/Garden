using System.Collections;

public class LoadObjectBlock : BlockBehaviour
{
    private ObjectMenuPluginsBase m_resourceMenu;
    private DataMenuPlugins m_variableMenu;

    protected override void Start()
    {
        base.Start();

        m_resourceMenu = GetComponentInChildren<ObjectMenuPluginsBase>(!NodeTemplateCache.Instance.ShowBlockUI);
        m_variableMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        if (m_resourceMenu.assetId != 0)
        {
            yield return CreateObject(m_resourceMenu.assetId);
        }
    }

    private IEnumerator CreateObject(int assetId)
    {
        var request = CodeContext.worldApi.CreateObject(assetId);
        yield return request;

        var variable = CodeContext.variableManager.get<VariableData>(m_variableMenu.GetMenuValue());
        int objectId = request.result;
        if (objectId != 0 && variable != null)
        {
            variable.setValue(objectId);
        }
    }
}
