using System.Collections;

public class RepeatForeverBlock : BlockBehaviour
{
	StepNode m_Step;

	protected override void Start()
	{
		base.Start();
		m_Step = GetComponent<StepNode>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        using (context.EnterLoop())
        {
            while (!context.shouldBreakFromLoop && !context.isAborted)
            {
                yield return m_Step.ActionStep(context, 0);
                // avoid infinite loop
                yield return null;
            }
        }
	}
}
