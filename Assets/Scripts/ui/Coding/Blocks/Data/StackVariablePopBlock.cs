using UnityEngine;
using System.Collections;

public class StackVariablePopBlock : VariableBaseStackBlock
{
	StackMenuPlugins m_Stack;
	VariableMenuPlugins m_Variable;

	protected override void Start()
	{
		base.Start();
		m_Stack = gameObject.GetComponentInChildren<StackMenuPlugins>();
		m_Variable = gameObject.GetComponentInChildren<VariableMenuPlugins>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var stackData = CodeContext.variableManager.get<StackData>(m_Stack.GetMenuValue());
        if (stackData != null && isWritable(stackData))
        {
            string tVariableName = m_Variable.GetMenuValue();
            string tRt = stackData.pop();
            if (null != tRt)
            {
                var varData = CodeContext.variableManager.get<VariableData>(tVariableName);
                if (varData != null)
                { 
                    varData.setValue(tRt);
                }
            }
        }

		yield break;
	}
}
