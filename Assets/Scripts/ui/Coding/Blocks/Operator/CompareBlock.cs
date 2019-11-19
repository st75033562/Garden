using System.Collections;
using System.Collections.Generic;

public class CompareBlock : InsertBlock
{
	ComparePlugins m_compare;

	protected override void Start()
	{
		base.Start();

		m_compare = GetComponentInChildren<ComparePlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		retValue.value = m_compare.Compare(slotValues[0], slotValues[1]);
	}
}
