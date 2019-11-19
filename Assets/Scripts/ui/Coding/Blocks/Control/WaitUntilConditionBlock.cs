using System.Collections;
using System.Collections.Generic;

public class WaitUntilConditionBlock : BlockBehaviour
{
	UntilWhilePlugins m_DownMenu;

	protected override void Start()
	{
		base.Start();
		m_DownMenu = GetComponentInChildren<UntilWhilePlugins>();
    }

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var slotValues = new List<string>();
        while (true)
        {
            yield return Node.GetSlotValues(context, slotValues);
            var repeatCond = "down_menu_loop_while" == m_DownMenu.GetMenuValue();

            if (BlockUtils.ParseBool(slotValues[0]) == repeatCond)
            {
                yield return null;
            }
            else
            {
                yield break;
            }
        }
	}
}
