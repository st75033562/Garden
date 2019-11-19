using System;
using System.Collections.Generic;
using UnityEngine;

public class PkAnswerSelectView : MonoBehaviour
{
    [SerializeField]
    private ScrollLoopController scroll;

    [SerializeField]
    private PKAnswerSelectConfirmDialog confirmDialog; 

    private Action<PKAnswer> callBack;

    public void SetData(List<PKAnswer> myAnswers, Action<PKAnswer> action)
    {
        scroll.context = this;
        scroll.initWithData(myAnswers);
        callBack = action;
    }

    public void OnClickCell(PkAnswerItem cell)
    {
        var answer = cell.pkAnswer;
        confirmDialog.gameObject.SetActive(true);
        confirmDialog.Initialize(cell.pkAnswer);
        confirmDialog.SetConfirmAction(() => {
            gameObject.SetActive(false);
            if (answer != null)
            {
                callBack(answer);
            }
        });
    }

    public void OnClickBack()
    {
        gameObject.SetActive(false);
    }
}
