using Robomation;

public class SetLedToColorNumberBlock : SetLedToColorBlockBase
{
    protected override int GetColorId(BlockState state)
    {
        int colorId;
        int.TryParse(state.slotValues[1], out colorId);
        if (colorId < Hamster.LED_OFF || colorId > Hamster.LED_WHITE)
        {
            return Hamster.LED_OFF;
        }
        return colorId;
    }
}
