using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerOfBlock : InsertBlock
{
	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		float value, power;
		float.TryParse(slotValues[0], out value);
		float.TryParse(slotValues[1], out power);
		retValue.value = Mathf.Pow(value, power).ToString();
	}
}
