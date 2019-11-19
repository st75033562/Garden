using System.Collections;
using System.Collections.Generic;

public class IfConditionThenBlock : BlockBehaviour
{
    StepNode m_Step;

    protected override void Start()
    {
        base.Start();
        m_Step = (StepNode)Node;
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return m_Step.GetSlotValues(context, slotValues);
        if (BlockUtils.ParseBool(slotValues[0]))
        {
            yield return m_Step.ActionStep(context, 0);
        }
    }
}
