using UnityEngine;
using System.Collections;

public class VibrateBlock : BlockBehaviour
{
	public override IEnumerator ActionBlock(ThreadContext context)
	{
#if UNITY_IOS || UNITY_ANDROID
		Handheld.Vibrate();
#endif
		yield break;
	}
}
