using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListVariableReplaceBlock : VariableBaseListBlock
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
            int pos;
            if ("down_menu_last".Equals(slotValues[0]))
            {
                pos = listData.size();
            }
            else if ("down_menu_random".Equals(slotValues[0]))
            {
                pos = Random.Range(1, listData.size() + 1);
            }
            else
            {
                int.TryParse(slotValues[0], out pos);
            }
            listData[pos] = slotValues[1];
        }
	}
}
