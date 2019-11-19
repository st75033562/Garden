using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupExercises : PermitUiBase {

    public GameObject[] contentPanels;

    protected override void Start() {
        base.Start();
    }

    public void SwitchPanel(GameObject go) {
        foreach(GameObject panel in contentPanels)//需要先执行 ondisable 
        {
            panel.SetActive(false);
        }
        go.SetActive(true);
    }
}
