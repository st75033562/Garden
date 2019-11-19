using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseTest : MonoBehaviour {
    public GameObject[] contentPanels;
    public GameObject studentTestGo;

    void Start() {
        if(UserManager.Instance.IsStudent) {
            SwitchPanel(studentTestGo);
        }       
    }

    public void SwitchPanel(GameObject go) {
        foreach(GameObject panel in contentPanels)//需要先执行 ondisable 
        {
            panel.SetActive(false);
        }
        go.SetActive(true);
    }
}
