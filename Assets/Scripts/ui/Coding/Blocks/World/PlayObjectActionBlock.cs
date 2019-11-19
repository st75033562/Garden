using System.Collections;
using System.Collections.Generic;

public class PlayObjectActionBlock : BlockBehaviour
{
    private DataMenuPlugins m_objectVarMenu;

    protected override void Start()
    {
        base.Start();

        m_objectVarMenu = GetComponentInChildren<VariableMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var objectId = CodeContext.variableManager.getVarInt(m_objectVarMenu.GetMenuValue());
        if (objectId != 0)
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            int actionId;
            if (int.TryParse(slotValues[0], out actionId))
            {
                CodeContext.worldApi.PlayAction(objectId, actionId, slotValues[1], slotValues[2]);
            }
        }
    }
}