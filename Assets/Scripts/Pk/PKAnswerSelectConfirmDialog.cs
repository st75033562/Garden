using System;
using UnityEngine;
using UnityEngine.UI;

public class PKAnswerSelectConfirmDialog : MonoBehaviour
{
    public Text m_nameText;
    public Text m_creationTimeText;
    private Action m_onConfirm;

    public void Initialize(PKAnswer answer)
    {
        if (answer == null)
        {
            throw new ArgumentNullException("answer");
        }
        m_nameText.text = answer.AnswerName;
        m_creationTimeText.text = answer.CreationTime.ToLocalTime()
                                        .ToString("ui_pk_select_answer_creation_time".Localize());
    }

    public void SetConfirmAction(Action action)
    {
        m_onConfirm = action;
    }

    public void OnClickConfirm()
    {
        gameObject.SetActive(false);
        if (m_onConfirm != null)
        {
            m_onConfirm();
            m_onConfirm = null;
        }
    }
}
