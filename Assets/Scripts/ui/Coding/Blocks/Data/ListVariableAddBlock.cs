using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListVariableAddBlock : VariableBaseListBlock
{
    ListMenuPlugins m_Menu;

    protected override void Start()
    {
        base.Start();
        m_Menu = gameObject.GetComponentInChildren<ListMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var listData = CodeContext.variableManager.get<ListData>(m_Menu.GetMenuValue());
        if (listData != null && isWritable(listData))
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            listData.add(slotValues[0]);
        }
    }
}
