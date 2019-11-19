using System.Collections;

public class AttachObjectToObjectBlock : BlockBehaviour
{
    private DataMenuPlugins[] m_menuPlugins;

    protected override void Start()
    {
        base.Start();

        m_menuPlugins = GetComponentsInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        yield return null;
        var sourceObjId = CodeContext.variableManager.getVarInt(m_menuPlugins[0].GetMenuValue());
        var targetObjId = CodeContext.variableManager.getVarInt(m_menuPlugins[1].GetMenuValue());
        CodeContext.worldApi.AttachObjectToObject(sourceObjId, targetObjId);
        yield break;
    }
}
