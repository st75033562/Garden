using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuotientOfBlock : InsertBlock
{
	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		int dividend, divisor, result = 0;
		int.TryParse(slotValues[0], out dividend);
		int.TryParse(slotValues[1], out divisor);
		if (0 != divisor)
		{
			result = dividend / divisor;
		}
		retValue.value = result.ToString();
	}
}
