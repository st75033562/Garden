using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExerciseCell : ScrollCell {
    public Text levelText;
    public Text downloadCount;
    public Text textCoin;
    public Text exerciseName;
    public GameObject mask;
    public ExerciseInfo exerciseInfo { get; set; }
    public override void configureCellData() {
        exerciseInfo = (ExerciseInfo)DataObject;
        levelText.text = PopupEditorExercises.levelLanguare[exerciseInfo.level].Localize() ;
        DownLoadCount = exerciseInfo.downLoadCount;
        if(textCoin != null) {
            textCoin.text = exerciseInfo.price.ToString();
        }
        exerciseName.text = exerciseInfo.exerciesName;
        ShowMask(exerciseInfo.showMask);
    }

    public void ShowMask(bool showMask) {
        mask.SetActive(showMask);
    }

    public uint DownLoadCount {
        set {
            exerciseInfo.downLoadCount = value;
            downloadCount.text = exerciseInfo.downLoadCount.ToString();
        }
        get {
            return exerciseInfo.downLoadCount;
        }
    }
}
