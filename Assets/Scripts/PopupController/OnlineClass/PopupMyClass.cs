using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupMyClass : PermitUiBase {
    public GameObject[] contentPanels;
    public GameObject joinPanel;
    // Use this for initialization
    protected override void Start () {
        base.Start();
        if(UserManager.Instance.IsStudent) {
            SwitchPanel(joinPanel);
        }
	}

    public void SwitchPanel(GameObject go) {
        foreach(GameObject panel in contentPanels) { //需要先调用 OnDisable 事件
            panel.SetActive(false);
        }
        go.SetActive(true);
    }
}
