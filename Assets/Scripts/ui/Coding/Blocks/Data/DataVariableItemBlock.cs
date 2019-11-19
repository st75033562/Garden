using System.Collections;

public class DataVariableItemBlock : VariableBaseVarBlock
{
	VariableMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = GetComponentInChildren<VariableMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        var varData = CodeContext.variableManager.get<VariableData>(m_Menu.GetMenuValue());
        retValue.value = varData != null ? varData.getString() : string.Empty;
        yield break;
	}
}
