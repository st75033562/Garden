using System.Collections;
using System.Collections.Generic;

public class RepeatTimesBlock : BlockBehaviour
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
        int loopCounter;
        int.TryParse(slotValues[0], out loopCounter);

        using (context.EnterLoop())
        {
            while (loopCounter-- > 0 && !context.shouldBreakFromLoop && !context.isAborted)
            {
                yield return m_Step.ActionStep(context, 0);
            }
        }
    }
}
