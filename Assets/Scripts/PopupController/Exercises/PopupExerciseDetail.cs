using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupExerciseDetail : PopupController {

    public class Payload {
        public bool showDownBtn;
        public ExerciseInfo exercieseInfo;
        public Action downloadBack;
    }

    public Text exerciseName;
    public Text exerciseDescre;
    public ScrollLoopController scroll;
    public GameObject downloadBtn;

    private Payload config;

    protected override void Start () {
        config = (Payload)payload;
        base.Start();
        exerciseName.text = config.exercieseInfo.exerciesName;
        exerciseDescre.text = config.exercieseInfo.exerciesDescripe;
        downloadBtn.SetActive(config.showDownBtn);

        scroll.initWithData(config.exercieseInfo.attachDatas);
    }

    public void OnClickDownLoad() {
        if(config.downloadBack != null) {
            config.downloadBack();
        }
    }
}
