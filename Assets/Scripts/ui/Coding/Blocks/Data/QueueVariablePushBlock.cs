using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QueueVariablePushBlock : VariableBaseQueueBlock
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
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            queueData.enqueue(slotValues[0]);
        }
    }
}
