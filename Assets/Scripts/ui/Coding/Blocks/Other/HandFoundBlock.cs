using Robomation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandFoundBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int index;
        int.TryParse(slotValues[0], out index);
        var robot = CodeContext.robotManager.get(index);
        if (robot != null)
        {
            if (robot.read(Hamster.LEFT_PROXIMITY) > 50
                || robot.read(Hamster.RIGHT_PROXIMITY) > 50)
            {
                retValue.value = "true";
                yield break;
            }
        }
        retValue.value = "false";
    }
}
