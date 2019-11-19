using System.Collections;
using System.Collections.Generic;

public class MoveTowardsBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int objectId, targetId;
        if (int.TryParse(slotValues[0], out objectId) &&
            int.TryParse(slotValues[1], out targetId))
        {
            CodeContext.worldApi.MoveTowards(objectId, targetId);
        }
    }
}
