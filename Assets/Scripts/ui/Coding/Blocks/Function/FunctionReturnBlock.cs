using System.Collections;
using System.Collections.Generic;

public class FunctionReturnBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        context.Return(slotValues[0]);
    }
}
