using UnityEngine;
using System.Linq;

public class PkRecord : MonoBehaviour
{
    [SerializeField]
    private ScrollLoopController scroll;

    [SerializeField]
    private PkAnswerSelectView answerSelectView;

    [SerializeField]
    private GameObject challengeButton;

    private PK pk;
    private PKAnswer answer;

    public void SetData(PK pk, PKAnswer answer)
    {
        Cleanup();
        this.pk = pk;
        this.answer = answer;
        this.answer.onPKResultAdded += OnResultAdded;
        scroll.context = this;
        scroll.initWithData(answer.PkResultList
                                  .Select(x => CreatePkRecordCellData(x)).ToList());

        challengeButton.SetActive(answer.AnswerUserId != UserManager.Instance.UserId && UserManager.Instance.IsStudent);
    }

    private PkRecordCellData CreatePkRecordCellData(PK_Result result)
    {
        var data = new PkRecordCellData();
        var challengerAnswer = pk.GetAnswer(result.ChanllengerAnswerId);
        var acceptorAnswer = pk.GetAnswer(result.AccepterId);

        // if the answer is a challenger
        if (challengerAnswer.AnswerUserId == answer.AnswerUserId)
        {
            data.rivalName = acceptorAnswer.AnswerNickname;
            data.rivalResult = result.AcceptorOutcome;
        }
        else
        {
            data.rivalName = challengerAnswer.AnswerNickname;
            data.rivalResult = result.ChallengerOutcome;
        }
        return data;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnResultAdded(PK_Result result)
    {
        scroll.add(CreatePkRecordCellData(result));
    }

    private void Cleanup()
    {
        if (answer != null)
        {
            answer.onPKResultAdded -= OnResultAdded;
        }
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    public void OnClickChallenge()
    {
        var helper = new PkChallengeHelper(answerSelectView, pk, answer);
        helper.Challenge();
    }
}
