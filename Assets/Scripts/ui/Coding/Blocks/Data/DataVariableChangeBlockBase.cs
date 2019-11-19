using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class DataVariableChangeBlockBase : VariableBaseVarBlock
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
            float changedValue;
            if (float.TryParse(slotValues[0], out changedValue) && varData.isNumber())
            {
                varData.addValue(IncreaseNumber ? changedValue : -changedValue);
            }
        }
    }

    protected abstract bool IncreaseNumber { get; }
}
