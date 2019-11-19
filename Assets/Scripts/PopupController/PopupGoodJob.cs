using UnityEngine;
using System.Collections;

public class PopupGoodJob : PopupController {

	// Use this for initialization
	protected override void Start () {
        StartCoroutine(DelayDestory());
	}

    IEnumerator DelayDestory() {
        yield return new WaitForSeconds(2);
        Close();
    }
}
