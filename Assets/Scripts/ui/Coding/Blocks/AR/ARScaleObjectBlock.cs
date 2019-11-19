using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARScaleObjectBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        var scale = new Vector3();
        int markerId;
        if (float.TryParse(slotValues[0], out scale.x) &&
            float.TryParse(slotValues[1], out scale.y) &&
            float.TryParse(slotValues[2], out scale.z) &&
            int.TryParse(slotValues[3], out markerId) &&
            markerId >= 0)
        {
            scale = Coordinates.ConvertVector(scale);
            CodeContext.arSceneManager.SetObjectScale(markerId, scale);
        }
    }
}
