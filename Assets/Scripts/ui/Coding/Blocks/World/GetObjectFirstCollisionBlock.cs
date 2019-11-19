using System.Collections;

public class GetObjectFirstCollisionBlock : BlockBehaviour
{
    private VariableMenuPlugins[] m_plugins;

    protected override void Start()
    {
        base.Start();

        m_plugins = GetComponentsInChildren<VariableMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var objId = CodeContext.variableManager.getVarInt(m_plugins[0].GetMenuValue());
        if (objId != 0)
        {
            var collidedObjId = CodeContext.worldApi.TakeObjectFirstCollidedObject(objId);
            CodeContext.variableManager.setVar(m_plugins[1].GetMenuValue(), collidedObjId);
        }

        yield break;
    }
}
