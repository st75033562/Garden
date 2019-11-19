using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StackVariablePushBlock : VariableBaseStackBlock
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
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            stackData.push(slotValues[0]);
        }
    }
}
