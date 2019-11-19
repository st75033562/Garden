using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftExerciseCell : ExerciseCell {

    public override void configureCellData() {
        base.configureCellData();
        downloadCount.gameObject.SetActive(false);
    }
}
