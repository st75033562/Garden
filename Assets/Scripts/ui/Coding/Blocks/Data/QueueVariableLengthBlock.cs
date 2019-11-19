using UnityEngine;
using System.Collections;

public class QueueVariableLengthBlock : VariableBaseQueueBlock
{
	QueueMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<QueueMenuPlugins>();
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        var queueData = CodeContext.variableManager.get<QueueData>(m_Menu.GetMenuValue());
        if (queueData != null)
        {
            retValue.value = queueData.size().ToString();
        }
        else
        {
            retValue.value = "0";
        }
        yield break;
	}
}
