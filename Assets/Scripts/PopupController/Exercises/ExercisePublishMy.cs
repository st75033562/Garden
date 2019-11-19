using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExercisePublishMy : BaseExercise {

	protected override void OnEnable () {
        topPublish.SetActive(false);
        topTestArea.SetActive(false);
        topAddGo.SetActive(false);
        base.OnEnable();
        SynchorExercise(GetTopicListType.GetTopicPublishedMyself);
    }
}
