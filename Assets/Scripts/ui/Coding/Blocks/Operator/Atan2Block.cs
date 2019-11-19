using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atan2Block : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        float dy, dx;
        if (float.TryParse(slotValues[0], out dy) &&
            float.TryParse(slotValues[1], out dx))
        {
            retValue.value = (Mathf.Atan2(dy, dx) * Mathf.Rad2Deg).ToString();
        }
        else
        {
            retValue.value = "0";
        }
    }
}
