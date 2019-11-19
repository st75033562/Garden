using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckAndroidUpdate : BaseCheckUpdate
{

    public override IEnumerator CheckUpdate()
    {
        platformName = "android";
        yield return base.CheckUpdate();
    }
}
