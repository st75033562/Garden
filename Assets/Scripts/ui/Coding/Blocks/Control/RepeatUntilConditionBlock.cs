using System.Collections;
using System.Collections.Generic;

public class RepeatUntilConditionBlock : BlockBehaviour
{
	StepNode m_Step;
	UntilWhilePlugins m_DownMenu;

	protected override void Start()
	{
		base.Start();
		m_Step = GetComponent<StepNode>();
		m_DownMenu = GetComponentInChildren<UntilWhilePlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        var isWhile = "down_menu_loop_while" == m_DownMenu.GetMenuValue();

        var slotValues = new List<string>();
        yield return m_Step.GetSlotValues(context, slotValues);
        using (context.EnterLoop())
        {
            while (BlockUtils.ParseBool(slotValues[0]) == isWhile &&
                    !context.shouldBreakFromLoop &&
                    !context.isAborted)
            {
                yield return m_Step.ActionStep(context, 0);
                yield return m_Step.GetSlotValues(context, slotValues);
                // avoid infinite loop
                yield return null;
            }
        }
	}
}
