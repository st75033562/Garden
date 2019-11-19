using System.Collections;
using System.Collections.Generic;

public class ARAction2ParamsBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int markerId;
        int actionId;
        if (int.TryParse(slotValues[0], out markerId) &&
            int.TryParse(slotValues[1], out actionId) && 
            markerId >= 0)
        {
            CodeContext.arSceneManager.DoAction(markerId, actionId, slotValues[2], slotValues[3]);
        }
    }
}
