using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITeacherSelectGrade : MonoBehaviour {
    public UITeacherGradeTask gradeTask;
    public Button[] gradesBtn;

    private int preIndex ;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClickClose() {
        if(preIndex != 0) {
            gradesBtn[preIndex - 1].interactable = true;
        }
        preIndex = 0;
        gameObject.SetActive(false);
    }

    public void OnClickConfirm() {
        if(preIndex != 0)
            gradeTask.ClickGradeBtn(preIndex);
        OnClickClose();
    }

    public void OnClickSelectGrade(int level) {
        if(preIndex != 0) {
            gradesBtn[preIndex - 1].interactable = true;
        }
        gradesBtn[level - 1].interactable = false;
        preIndex = level;
    }
}
