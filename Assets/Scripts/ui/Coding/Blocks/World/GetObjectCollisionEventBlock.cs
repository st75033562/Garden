using System.Collections;
using System.Linq;

public class GetObjectCollisionEventBlock : BlockBehaviour
{
    private VariableMenuPlugins m_varMenu;
    private ListMenuPlugins m_listMenu;

    protected override void Start()
    {
        base.Start();

        m_varMenu = GetComponentInChildren<VariableMenuPlugins>();
        m_listMenu = GetComponentInChildren<ListMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        int objectId = CodeContext.variableManager.getVarInt(m_varMenu.GetMenuValue());
        if (objectId != 0)
        {
            var list = CodeContext.variableManager.get<ListData>(m_listMenu.GetMenuValue());
            var objectIds = CodeContext.worldApi.GetObjectCollidedObjects(objectId, true);
            if (list != null)
            {
                list.reset();
                list.add(objectIds.Select(x => x.ToString()));
            }
        }
        yield break;
    }
}
