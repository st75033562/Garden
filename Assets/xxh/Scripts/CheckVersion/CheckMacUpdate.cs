using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMacUpdate : BaseCheckUpdate
{
    public override IEnumerator CheckUpdate()
    {
        platformName = "osx";
        yield return base.CheckUpdate();
    }   
}
