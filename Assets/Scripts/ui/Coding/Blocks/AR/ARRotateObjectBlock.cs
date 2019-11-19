using RobotSimulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARRotateObjectBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        float degrees;
        int markerId;
        if (float.TryParse(slotValues[0], out degrees) &&
            int.TryParse(slotValues[1], out markerId) &&
            markerId >= 0)
        {
            CodeContext.arSceneManager.SetObjectRotation(markerId, new Vector3(0.0f, -degrees, 0.0f));
        }
    }
}
