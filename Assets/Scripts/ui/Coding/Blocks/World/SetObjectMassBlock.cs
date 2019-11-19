using System.Collections;
using System.Collections.Generic;

public class SetObjectMassBlock : BlockBehaviour
{
    private DownMenuPlugins m_varMenu;

    protected override void Start()
    {
        base.Start();

        m_varMenu = GetComponentInChildren<DownMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        float mass;
        if (float.TryParse(slotValues[0], out mass))
        {
            var objId = CodeContext.variableManager.getVarInt(m_varMenu.GetMenuValue());
            CodeContext.worldApi.SetObjectMass(objId, mass);
        }
    }
}
