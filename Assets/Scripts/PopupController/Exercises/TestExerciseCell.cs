using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestExerciseCell : ExerciseCell {
    public GameObject coinGo;
    public override void configureCellData() {
        base.configureCellData();
        coinGo.SetActive(false);
    }
}
