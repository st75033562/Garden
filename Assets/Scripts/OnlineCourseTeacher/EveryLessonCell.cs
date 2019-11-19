using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EveryLessonCell : DragUI {
    [SerializeField]
    private GameObject contentGo;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private GameObject operationGo;

    private Period_Info periodInfo;

    public override void configureCellData() {
        periodInfo = DataObject as Period_Info;
        operationGo.SetActive(false);
        if(periodInfo == null) {
            contentGo.SetActive(false);
        } else {
            contentGo.SetActive(true);
            nameText.text = periodInfo.PeriodName;
        }
        UpdateOperation();
    }

    public override void EndDragRaycast(GameObject go) {
        
    }

    public void OnClickCell() {
        if(periodInfo.periodOperation == PeriodOperation.NONE) {
            OnClickEditor();
        } else {
            OnClickDel();
        }
    }

    void OnClickDel() {
        PopupManager.YesNo("ui_confirm_delete".Localize(),
            ()=> {
                (Context as PopupILPeriod).ClickDelPeriod(periodInfo);
            }, null);
    }

    void OnClickEditor() {
        (Context as PopupILPeriod).ClickShowCreateLesson(periodInfo);
    }

    public void UpdateOperation() {
        if(periodInfo != null) {
            operationGo.SetActive(periodInfo.periodOperation != PeriodOperation.NONE);
        }
    }
}
