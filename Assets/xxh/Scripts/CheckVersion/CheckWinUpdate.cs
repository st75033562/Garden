using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckWinUpdate : BaseCheckUpdate
{

    public override IEnumerator CheckUpdate()
    {
        platformName = "win";
        yield return base.CheckUpdate();
    }
}
