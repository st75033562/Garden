using System.Collections;

public class GetObjectTransformBlock : BlockBehaviour
{
    private DataMenuPlugins[] m_variableMenus;

    protected override void Start()
    {
        base.Start();

        m_variableMenus = GetComponentsInChildren<VariableMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var objectId = CodeContext.variableManager.getVarInt(m_variableMenus[0].GetMenuValue());
        if (objectId != 0)
        {
            var pos = CodeContext.worldApi.GetObjectPosition(objectId);
            CodeContext.variableManager.setVar(m_variableMenus[1].GetMenuValue(), pos.x);
            CodeContext.variableManager.setVar(m_variableMenus[2].GetMenuValue(), pos.y);
            CodeContext.variableManager.setVar(m_variableMenus[3].GetMenuValue(), pos.z);

            var rotation = CodeContext.worldApi.GetObjectRotation(objectId);
            CodeContext.variableManager.setVar(m_variableMenus[4].GetMenuValue(), rotation);
        }

        yield break;
    }

}
