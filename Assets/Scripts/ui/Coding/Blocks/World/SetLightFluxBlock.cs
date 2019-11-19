using System.Collections;
using System.Collections.Generic;

public class SetLightFluxBlock : BlockBehaviour
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

        int objectId = CodeContext.variableManager.getVarInt(m_varMenu.GetMenuValue());
        int flux;
        int.TryParse(slotValues[0], out flux);

        CodeContext.worldApi.SetLightFlux(objectId, flux);
    }
}
