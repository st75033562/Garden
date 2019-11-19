using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListVariableContainBlock : VariableBaseListBlock
{
    ListMenuPlugins m_Menu;

    protected override void Start()
    {
        base.Start();
        m_Menu = gameObject.GetComponentInChildren<ListMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        bool result = false;
        var listData = CodeContext.variableManager.get<ListData>(m_Menu.GetMenuValue());
        if (listData != null)
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            result = listData.contains(slotValues[0]);
        }
        retValue.value = result.ToString();
    }
}
