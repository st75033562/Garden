using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GetRobotCollisionEventBlock : BlockBehaviour
{
    private ListMenuPlugins m_listMenu;

    protected override void Start()
    {
        base.Start();

        m_listMenu = GetComponentInChildren<ListMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int robotIndex;
        if (int.TryParse(slotValues[0], out robotIndex))
        {
            var list = CodeContext.variableManager.get<ListData>(m_listMenu.GetMenuValue());
            var objectIds = CodeContext.worldApi.GetRobotCollidedObjects(robotIndex, true);
            if (list != null)
            {
                list.reset();
                list.add(objectIds.Select(x => x.ToString()));
            }
        }
    }
}
