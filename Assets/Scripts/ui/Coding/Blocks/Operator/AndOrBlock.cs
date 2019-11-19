using System.Collections;
using System.Collections.Generic;

public class AndOrBlock : InsertBlock
{
    AndOrPlugins m_andor;

    protected override void Start()
    {
        base.Start();

        m_andor = GetComponentInChildren<AndOrPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        retValue.value = m_andor.Action(slotValues[0], slotValues[1]);
    }
}
