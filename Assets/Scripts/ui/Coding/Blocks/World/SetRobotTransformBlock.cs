using System.Collections;
using UnityEngine;

public class SetRobotTransformBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        float x, y, theta;
        if (float.TryParse(state.slotValues[1], out x) &&
            float.TryParse(state.slotValues[2], out y) &&
            float.TryParse(state.slotValues[3], out theta))
        {
            CodeContext.worldApi.SetRobotPosition(state.robotIndex, new Vector2(x, y));
            CodeContext.worldApi.SetRobotRotaton(state.robotIndex, theta);
        }

        yield break;
    }
}
