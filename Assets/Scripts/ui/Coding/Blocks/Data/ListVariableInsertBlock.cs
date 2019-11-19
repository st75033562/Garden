using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListVariableInsertBlock : VariableBaseListBlock
{
    ListMenuPlugins m_Menu;

    protected override void Start()
    {
        base.Start();
        m_Menu = gameObject.GetComponentInChildren<ListMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var listData = CodeContext.variableManager.get<ListData>(m_Menu.GetMenuValue());
        if (listData != null && isWritable(listData))
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            int pos;
            if ("down_menu_last".Equals(slotValues[1]))
            {
                pos = listData.size() + 1;
            }
            else if ("down_menu_random".Equals(slotValues[1]))
            {
                pos = Random.Range(1, listData.size() + 2);
            }
            else
            {
                int.TryParse(slotValues[1], out pos);
            }

            listData.insert(pos, slotValues[0]);
        }
    }
}
