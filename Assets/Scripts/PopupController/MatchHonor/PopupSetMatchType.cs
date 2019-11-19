using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSetMatchType : PopupController {

    private Action<Course_Race_Type> action;

	protected override void Start () {
        base.Start();

        action = (Action<Course_Race_Type>)payload;
    }

    public void OnClickSelectMode(int mode) {
        action((Course_Race_Type)mode);
        Close();
    }
}
