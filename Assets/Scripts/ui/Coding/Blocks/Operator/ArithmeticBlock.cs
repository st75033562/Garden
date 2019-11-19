using System.Collections;
using System.Collections.Generic;

public class ArithmeticBlock : InsertBlock
{
	CalculatePlugins m_calculate;

	protected override void Start()
	{
		base.Start();

		m_calculate = GetComponentInChildren<CalculatePlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		retValue.value = m_calculate.Calculate(slotValues[0], slotValues[1]);
	}
}
