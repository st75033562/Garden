using System.Collections;

public class SetTempoToBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        int tempo;
        int.TryParse(state.slotValues[1], out tempo);
        state.runtimeState.tempo = tempo;

        yield break;
    }
}
