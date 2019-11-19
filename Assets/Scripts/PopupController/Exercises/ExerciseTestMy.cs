using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseTestMy : BaseExercise {
    protected override void OnEnable() {
        base.OnEnable();
        topAddGo.SetActive(false);
        topTestArea.SetActive(false);
        topPublish.SetActive(false);

        SynchorExercise(GetTopicListType.GetTopicInvitedMyself);
    }

}
