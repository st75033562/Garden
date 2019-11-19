public class SetLedToColorBlock : SetLedToColorBlockBase
{
	SelectColorPlugins m_LedColor;

	protected override void Start()
	{
		base.Start();
		m_LedColor = GetComponentInChildren<SelectColorPlugins>();
	}

    protected override int GetColorId(BlockState state)
    {
        return (int)m_LedColor.colorId;
    }
}
