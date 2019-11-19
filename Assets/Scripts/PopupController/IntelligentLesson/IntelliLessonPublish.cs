using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelliLessonPublish : MonoBehaviour {
    public GameObject[] contentPanels;
    public GameObject myJoinGo;

    void OnEnable()
    {
        NodeTemplateCache.Instance.ShowBlockUI = false;
    }
    void Start() {
        if(UserManager.Instance.IsStudent) {
            SwitchPanel(myJoinGo);
        }
    }

    public void SwitchPanel(GameObject go) {
        foreach(GameObject panel in contentPanels) { //需要先调用 OnDisable 事件
            panel.SetActive(false);
        }
        go.SetActive(true);
    }
}
