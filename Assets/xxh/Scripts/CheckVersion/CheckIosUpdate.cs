using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIosUpdate : BaseCheckUpdate
{

    public override IEnumerator CheckUpdate()
    {
        platformName = "ios";
        yield return base.CheckUpdate();
    }
}
