using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NotBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        bool result = BlockUtils.ParseBool(slotValues[0]);
        retValue.value = !result ? "true" : "false";
    }
}
