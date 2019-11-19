public class DataVariableChangeBlock : DataVariableChangeBlockBase
{
    private ChangeDataPlugins m_changeMenu;

    protected override void Start()
    {
        base.Start();

        m_changeMenu = GetComponentInChildren<ChangeDataPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    protected override bool IncreaseNumber
    {
        get
        {
            return m_changeMenu.GetMenuValue() == "down_menu_data_variable_add";
        }
    }
}
