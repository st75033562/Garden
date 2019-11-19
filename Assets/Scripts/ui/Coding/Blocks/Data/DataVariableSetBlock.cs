using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataVariableSetBlock : VariableBaseVarBlock
{
    VariableMenuPlugins m_Menu;

    protected override void Start()
    {
        base.Start();
        m_Menu = gameObject.GetComponentInChildren<VariableMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var varData = CodeContext.variableManager.get<VariableData>(m_Menu.GetMenuValue());
        if (varData != null && isWritable(varData))
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            varData.setValue(slotValues[0]);
        }
    }
}
