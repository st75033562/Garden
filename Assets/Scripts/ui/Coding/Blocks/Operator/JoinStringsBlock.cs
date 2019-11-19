using System.Collections;
using System.Collections.Generic;

public class JoinStringsBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        retValue.value = slotValues[0] + slotValues[1];
    }
}
