using UnityEngine;
using System.Collections;

public class QueueVariableClearBlock : VariableBaseQueueBlock
{
	QueueMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = gameObject.GetComponentInChildren<QueueMenuPlugins>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var queueData = CodeContext.variableManager.get<QueueData>(m_Menu.GetMenuValue());
        if (queueData != null && isWritable(queueData))
        {
            queueData.reset();
        }
		yield break;
	}
}
