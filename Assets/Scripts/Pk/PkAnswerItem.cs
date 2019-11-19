using UnityEngine;
using UnityEngine.UI;

public class PkAnswerItem : ScrollCell
{
    [SerializeField]
    private Text textName;

    public PKAnswer pkAnswer
    {
        get { return (PKAnswer)DataObject; }
    }

    public override void configureCellData()
    {
        textName.text = pkAnswer.AnswerName;
    }
}
