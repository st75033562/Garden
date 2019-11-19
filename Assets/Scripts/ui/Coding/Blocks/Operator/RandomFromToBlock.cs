using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomFromToBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        int start, end;
        int.TryParse(slotValues[0], out start);
        int.TryParse(slotValues[1], out end);

        if (start > end)
        {
            int tmp = start;
            start = end;
            end = tmp;
        }

        retValue.value = Random.Range(start, end + 1).ToString();
    }
}
