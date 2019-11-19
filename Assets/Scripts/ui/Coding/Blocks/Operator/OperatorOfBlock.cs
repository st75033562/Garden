using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OperatorOfBlock : InsertBlock
{
	DownMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		float operand;
		float.TryParse(slotValues[0], out operand);
		retValue.value = Calculate(m_Menu.GetMenuValue(), operand);
	}

	public string Calculate(string method, float val)
	{
        float result = 0.0f;
		if(method.Equals("operator_absolute"))
		{
			result = Mathf.Abs(val);
		}
		else if(method.Equals("operator_round"))
		{
            result = Mathf.FloorToInt(val + 0.5f);
		}
		else if (method.Equals("operator_ceiling"))
		{
			result = Mathf.Ceil(val);
		}
		else if (method.Equals("operator_floor"))
		{
			result = Mathf.Floor(val);
		}
		else if (method.Equals("operator_sqrt"))
		{
			result = Mathf.Sqrt(val);
		}
		else if (method.Equals("operator_sin"))
		{
			result = Mathf.Sin(val * Mathf.Deg2Rad);
		}
		else if (method.Equals("operator_cos"))
		{
			result = Mathf.Cos(val * Mathf.Deg2Rad);
		}
		else if (method.Equals("operator_tan"))
		{
			result = Mathf.Tan(val * Mathf.Deg2Rad);
		}
		else if (method.Equals("operator_asin"))
		{
			result = Mathf.Asin(val) * Mathf.Rad2Deg;
		}
		else if (method.Equals("operator_acos"))
		{
			result = Mathf.Acos(val) * Mathf.Rad2Deg;
		}
		else if (method.Equals("operator_atan"))
		{
			result = Mathf.Atan(val) * Mathf.Rad2Deg;
		}
		else if (method.Equals("operator_log"))
		{
			result = Mathf.Log10(val);
		}
		else if (method.Equals("operator_ln"))
		{
			result = Mathf.Log(val);
		}

        return result.ToString();
	}
}
