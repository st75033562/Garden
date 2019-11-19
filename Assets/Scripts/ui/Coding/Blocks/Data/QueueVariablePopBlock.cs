using UnityEngine;
using System.Collections;

public class QueueVariablePopBlock : VariableBaseQueueBlock
{
	QueueMenuPlugins m_Queue;
	VariableMenuPlugins m_Variable;

	protected override void Start()
	{
		base.Start();
		m_Queue = gameObject.GetComponentInChildren<QueueMenuPlugins>();
		m_Variable = gameObject.GetComponentInChildren<VariableMenuPlugins>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var queueData = CodeContext.variableManager.get<QueueData>(m_Queue.GetMenuValue());
        if (queueData != null && isWritable(queueData))
        {
            string tVariableName = m_Variable.GetMenuValue();
            string tRt = queueData.dequeue();
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
