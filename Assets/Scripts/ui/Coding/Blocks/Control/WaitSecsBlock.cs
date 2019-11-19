using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaitSecsBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        float seconds;
        float.TryParse(slotValues[0], out seconds);
        yield return new WaitForSeconds(seconds);
    }
}
