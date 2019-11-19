using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ePowerOfBlock : InsertBlock
{
	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);

		float power;
		float.TryParse(slotValues[0], out power);

		retValue.value = Mathf.Exp(power).ToString();
	}
}
