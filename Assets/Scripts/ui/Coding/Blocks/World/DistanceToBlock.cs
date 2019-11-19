using System.Collections;
using System.Collections.Generic;

public class DistanceToBlock : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int fromObjId, toObjId;
        if (int.TryParse(slotValues[0], out fromObjId) &&
            int.TryParse(slotValues[1], out toObjId))
        {
            var fromObjPos = CodeContext.worldApi.GetObjectPosition(fromObjId);
            var toObjPos = CodeContext.worldApi.GetObjectPosition(toObjId);

            retValue.value = (fromObjPos - toObjPos).magnitude.ToString();
        }
        else
        {
            retValue.value = "0";
        }
    }
}
