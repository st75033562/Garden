using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrateForSecsBlock : BlockBehaviour
{
	public override IEnumerator ActionBlock(ThreadContext context)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		float seconds = 0;
		float.TryParse(slotValues[0], out seconds);
        int count = 0;
		if(seconds > 0)
		{
			count =  (int)(seconds / 0.4);
		}

		for (int i = 0; i < count; ++i)
		{
#if UNITY_IOS || UNITY_ANDROID
			Handheld.Vibrate();
#endif
			yield return new WaitForSeconds(0.4f);
		}
	}
}
