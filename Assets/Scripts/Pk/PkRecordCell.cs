using System;
using UnityEngine;
using UnityEngine.UI;

public class PkRecordCellData
{
    public PK_Result.Outcome rivalResult
    {
        get;
        set;
    }

    public string rivalName
    {
        get;
        set;
    }
}

public class PkRecordCell : ScrollCell
{
    [SerializeField]
    private Text textOrderNum;
    [SerializeField]
    private Text textRivalName;
    [SerializeField]
    private Text textResult;
    [SerializeField]
    private Image backgroundImage;

    private static readonly Color AlternateBgColor = new Color(0.95f, 0.95f, 0.95f);

    public override void configureCellData()
    {
        textOrderNum.text = (DataIndex + 1).ToString();
        backgroundImage.color = DataIndex % 2 == 0 ? Color.white : AlternateBgColor;

        var record = (PkRecordCellData)DataObject;
        textRivalName.text = record.rivalName;
        switch (record.rivalResult)
        {
        case PK_Result.Outcome.Win:
            textResult.text = "ui_pk_win".Localize();
            break;
        case PK_Result.Outcome.Lose:
            textResult.text = "ui_pk_lose".Localize();
            break;
        case PK_Result.Outcome.Draw:
            textResult.text = "ui_pk_draw".Localize();
            break;
        }
    }
}
