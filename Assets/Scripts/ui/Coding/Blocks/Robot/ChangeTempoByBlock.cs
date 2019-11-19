using System.Collections;

public class ChangeTempoByBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        int deltaTempo;
        int.TryParse(state.slotValues[1], out deltaTempo);
        state.runtimeState.tempo += deltaTempo;
        yield break;
    }
}
