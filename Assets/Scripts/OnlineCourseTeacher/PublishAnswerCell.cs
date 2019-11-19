using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishAnswerCell : ScrollCell {
    public GameObject[] operationGo;
    public Text Score;
    public Text answerName;
    public GameObject btnEditor;

    public override void configureCellData() {
        foreach (GameObject go in operationGo)
        {
            go.SetActive(false);
        }
        btnEditor.SetActive(Preference.scriptLanguage == ScriptLanguage.Visual);
        GBAnswer gbAnswer = (GBAnswer)DataObject;
        Score.text = gbAnswer.GbScore.ToString() + "ui_text_point".Localize();
        answerName.text = gbAnswer.AnswerName;
    }

    public void OnClickCell() {
        foreach(GameObject go in operationGo) {
            go.SetActive(!go.activeSelf);
        }
    }
}
