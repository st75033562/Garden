using UnityEngine;
using System.Collections;

public class ListVariableClearBlock : VariableBaseListBlock
{
	ListMenuPlugins m_Menu;

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var listData = CodeContext.variableManager.get<ListData>(m_Menu.GetMenuValue());
        if (listData != null && isWritable(listData))
        {
            listData.reset();
        }
        yield break;
	}

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<ListMenuPlugins>();
	}
}
