using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseDraft : BaseExercise {  
    public GameObject centerAddGo;
    
    protected override void OnEnable() {
        base.OnEnable();
        topAddGo.SetActive(true);
        topTestArea.SetActive(true);
        topPublish.SetActive(true);

        SynchorExercise(GetTopicListType.GetTopicDraft);
    }
    protected override void UpdateExercise() {
        base.UpdateExercise();
        centerAddGo.SetActive(exerciseInfos.Count == 0);
    }
    public void OnClickAddExercise() {
        if(gameObject.activeSelf) {
            PopupEditorExercises.PayLoad data = new PopupEditorExercises.PayLoad();
            data.baseExercise = this;
            PopupManager.EditorExercises(data, ()=> {
                SynchorExercise(GetTopicListType.GetTopicDraft);
            });
        }
    }

    public override void OnClickCell(ExerciseCell exerciseCell) {
        if(operationState == OperationState.Test) {
            PopupManager.SetPassword("ui_minicourse_view_password".Localize(), "", new SetPasswordData((str) => {
                SetTopicStatus(exerciseCell.exerciseInfo.id, str, 0);
            }, null));
        } else if(operationState == OperationState.Publish) {
            PopupManager.YesNo("ui_text_to_release".Localize(), ()=> {
                SetTopicStatus(exerciseCell.exerciseInfo.id, null, 0);
            });
        } else if(operationState == OperationState.Default) {
            PopupEditorExercises.PayLoad data = new PopupEditorExercises.PayLoad();
            data.exerciseInfo = exerciseCell.exerciseInfo;
            data.baseExercise = this;
            PopupManager.EditorExercises(data, () => {
                scrollContent.updateCellData(exerciseCell.exerciseInfo);
            });
        } else {
            base.OnClickCell(exerciseCell);
        }
    }

    void SetTopicStatus(uint topicid, string password, uint price) {
        var setTopicStatusR = new CMD_Set_Topic_Status_r_Parameters();
        setTopicStatusR.TopicId = topicid;
        if(operationState == OperationState.Publish) {
            setTopicStatusR.TopicStatus = Topic_Status.Publish;
        } else if(operationState == OperationState.Test) {
            setTopicStatusR.TopicStatus = Topic_Status.Test;
        }
        if(password != null) {
            setTopicStatusR.TopicPassword = password;
        }
        setTopicStatusR.TopicPrice = price;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdSetTopicStatusR, setTopicStatusR.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                PopupManager.Notice("ui_publish_sucess".Localize());
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public override ExerciseInfo GetExerciseByName(string name) {
        var exercise = exerciseInfos.Find(x => { return x.exerciesName == name; });
        return exercise;
    }

    public void OnClickPublish() {
        operationState = OperationState.Publish;
        SwitchMask(true);
    }

    public void OnClickTest() {
        operationState = OperationState.Test;
        SwitchMask(true);
    }
}
