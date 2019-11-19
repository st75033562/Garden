using UnityEngine;
using System.Collections;

public class StackVariableLengthBlock : VariableBaseStackBlock
{
	StackMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<StackMenuPlugins>();
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        var stackData = CodeContext.variableManager.get<StackData>(m_Menu.GetMenuValue());
        if (stackData != null)
        {
            retValue.value = stackData.size().ToString();
        }
        else
        {
            retValue.value = "0";
        }
        yield break;
	}
}
