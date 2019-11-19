public class DownMenuWithHintPlugins : DownMenuPlugins
{
    private bool m_selected;

    protected override void OnInput(string str)
    {
        base.OnInput(str);
        m_selected = true;
    }

    public override string GetMenuValue()
    {
        return m_selected ? m_TextKey : null;
    }

    protected void Deselect()
    {
        m_selected = false;
    }
}
