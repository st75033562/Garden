using System.Collections;
using System.Collections.Generic;

public class WhenConditionBlock : LoopMainBlock
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        while (true)
        {
            yield return Node.GetSlotValues(context, slotValues);
            if (BlockUtils.ParseBool(slotValues[0]))
            {
                yield break;
            }
            else
            {
                yield return null;
            }
        }
    }
}
