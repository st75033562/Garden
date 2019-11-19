using UnityEngine;
using System.Collections;

public class StackVariableClearBlock : VariableBaseStackBlock
{
	StackMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<StackMenuPlugins>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var stackData = CodeContext.variableManager.get<StackData>(m_Menu.GetMenuValue());
        if (stackData != null && isWritable(stackData))
        {
            stackData.reset();
        }
		yield break;
	}
}
