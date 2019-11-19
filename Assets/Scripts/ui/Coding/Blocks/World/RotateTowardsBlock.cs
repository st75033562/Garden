using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int objectId;
        float x, y;
        if (int.TryParse(slotValues[0], out objectId) &&
            float.TryParse(slotValues[1], out x) &&
            float.TryParse(slotValues[2], out y))
        {
            CodeContext.worldApi.RotateTowards(objectId, new Vector2(x, y));
        }
    }
}