using UnityEngine;
using System.Collections;

public class ListVariableLengthBlock : VariableBaseListBlock
{
	ListMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<ListMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        var listData = CodeContext.variableManager.get<ListData>(m_Menu.GetMenuValue());
        if (listData != null)
        {
            retValue.value = listData.size().ToString();
        }
        else
        {
            retValue.value = "0";
        }
        yield break;
	}
}
