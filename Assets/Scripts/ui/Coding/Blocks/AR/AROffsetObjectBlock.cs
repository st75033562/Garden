using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AROffsetObjectBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        var offset = new Vector3();
        int markerID;
        if (float.TryParse(slotValues[0], out offset.x) &&
            float.TryParse(slotValues[1], out offset.y) &&
            float.TryParse(slotValues[2], out offset.z) &&
            int.TryParse(slotValues[3], out markerID) &&
            markerID >= 0)
        {
            offset = Coordinates.ConvertVector(offset);
            CodeContext.arSceneManager.SetObjectOffset(markerID, offset);
        }
    }
}
