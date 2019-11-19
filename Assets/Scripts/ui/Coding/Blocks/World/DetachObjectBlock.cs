using System.Collections;

public class DetachObjectBlock : BlockBehaviour
{
    private DataMenuPlugins m_objectVar;

    protected override void Start()
    {
        base.Start();
        m_objectVar = GetComponentInChildren<DataMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        int objectId = CodeContext.variableManager.getVarInt(m_objectVar.GetMenuValue());
        CodeContext.worldApi.DetachObject(objectId);

        yield break;
    }
}
