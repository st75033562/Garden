using System.Collections;
using System.Collections.Generic;

public class MoveObjectBlock : BlockBehaviour
{
    private DownMenuPlugins m_varMenu;

    protected override void Start()
    {
        base.Start();
        m_varMenu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        var objId = CodeContext.variableManager.getVarInt(m_varMenu.GetMenuValue());
        float angularSpeed, linearSpeed;
        if (float.TryParse(slotValues[0], out angularSpeed) &&
            float.TryParse(slotValues[1], out linearSpeed))
        {
            CodeContext.worldApi.MoveObject(objId, angularSpeed, linearSpeed);
        }
    }
}
