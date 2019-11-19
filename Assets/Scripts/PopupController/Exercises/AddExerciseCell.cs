using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddExerciseCell : ScrollCell {
    public Text exerciseName;
    public Text teacherName;
    public Text description;
    public Text price;
    public Button btnBuy;
    public Text levelText;
    public Text buyText;

    public Topic_Info topicInfo;

    public override void configureCellData() {
        topicInfo = (Topic_Info)DataObject;
        exerciseName.text = topicInfo.TopicName;
        teacherName.text = topicInfo.TopicCreaterNickname;
        description.text = topicInfo.TopicDescription;
        price.text = topicInfo.TopicPrice.ToString();
        levelText.text = PopupEditorExercises.levelLanguare[topicInfo.TopicLevel].Localize();

        SetBuyState(!UserManager.Instance.AttendTopics.ContainsKey(topicInfo.TopicId));
    }

    public void SetBuyState(bool state) {
        btnBuy.interactable = state;
        if(state) {
            if(addExercise.type == PopupAddExercise.Type.Publish)
                buyText.text = "ui_course_buy".Localize();
            else
                buyText.text = "ui_add".Localize();
        } else {
            if(addExercise.type == PopupAddExercise.Type.Publish)
                buyText.text = "ui_course_purchased".Localize();
            else
                buyText.text = "ui_added".Localize();
        }
    }

    PopupAddExercise addExercise {
        get { return (PopupAddExercise)Context; }
    }
}
