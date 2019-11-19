using Google.Protobuf;
using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PkAnswersView : MonoBehaviour
{
    [SerializeField]
    private ScrollLoopController allAnswersScroll;
    [SerializeField]
    private ScrollLoopController myAnswersScroll;
    [SerializeField]
    private PkAnswerSelectView answerSelectView;
    [SerializeField]
    private PkRecord pkRecord;
    [SerializeField]
    private GameObject emptyGo;

    [SerializeField]
    private Toggle allAnswersToggle;
    [SerializeField]
    private Toggle myAnswersToggle;

    public UISortMenuWidget sortMenu;
    public GameObject tabGroup;
    public GameObject addButton;

    public PK pk { get; private set; }

    public enum SortKey
    {
        CreationTime,
        //AnswerName,
        Username,
        Win
    }

    private static readonly string[] s_sortOptions = {
        "ui_multi_pk_sort_creation_time",
        //"ui_multi_pk_sort_name",
        "ui_multi_pk_sort_username",
        "ui_multi_pk_sort_win"
    };

    private List<PKAnswerCellData> allAnswers;
    private List<PKAnswerCellData> myAnswers;

    private UISortSetting sortSetting;

    void Awake()
    {
        sortMenu.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
    }

    public void SetData(PK pk)
    {
        this.pk = pk;

        sortSetting = PKSortSetting.Get(UserManager.Instance, (int)pk.PkId);

        tabGroup.SetActive(!isMyPk);
        addButton.SetActive(!isMyPk);

        allAnswers = ToPKAnswerCellData(pk.PkAnswerList.Values);
        allAnswersScroll.context = this;

        myAnswers = ToPKAnswerCellData(pk.GetUserAnswers(UserManager.Instance.UserId));
        myAnswersScroll.context = this;

        sortMenu.onSortChanged.RemoveListener(OnSortChanged);
        sortMenu.SetCurrentSort((int)sortSetting.sortKey, sortSetting.ascending);
        sortMenu.onSortChanged.AddListener(OnSortChanged);
        OnSortChanged();

        // manually set all toggles as the group logic does not work when toggles are inactive
        allAnswersToggle.isOn = true;
        myAnswersToggle.isOn = false;

        ShowAllAnswers();
        pk.onAnswerAdded += OnAnswerAdded;
    }

    private bool isMyPk
    {
        get { return UserManager.Instance.UserId == pk.PkCreateId; }
    }

    private List<PKAnswerCellData> ToPKAnswerCellData(IEnumerable<PKAnswer> answers)
    {
        return answers.Select(x => ToPKAnswerCellData(x)).ToList();
    }

    private PKAnswerCellData ToPKAnswerCellData(PKAnswer answer)
    {
        return new PKAnswerCellData {
            canChallenge = answer.AnswerUserId != UserManager.Instance.UserId && !isMyPk,
            pkAnswer = answer,
        };
    }

    private void Sort(List<PKAnswerCellData> answers)
    {
        Comparison<PKAnswerCellData> comp = null;
        switch ((SortKey)sortSetting.sortKey)
        {
        case SortKey.CreationTime:
            comp = (lhs, rhs) => lhs.pkAnswer.CreationTime.CompareTo(rhs.pkAnswer.CreationTime);
            break;

        //case SortCriterion.AnswerName:
        //    comp = (lhs, rhs) => lhs.AnswerName.CompareWithUICulture(rhs.AnswerName);
        //    break;

        case SortKey.Username:
            comp = (lhs, rhs) => lhs.pkAnswer.AnswerNickname.CompareWithUICulture(rhs.pkAnswer.AnswerNickname);
            break;

        case SortKey.Win:
            comp = (lhs, rhs) => lhs.pkAnswer.PkWinCount.CompareTo(rhs.pkAnswer.PkWinCount);
            break;

        default:
            throw new InvalidOperationException();
        }

        answers.Sort(comp.Invert(!sortSetting.ascending));
    }

    private void OnSortChanged()
    {
        sortSetting.SetSortCriterion((int)sortMenu.activeSortOption, sortMenu.sortAsc);
        RefreshAllAnswsers();
        RefreshMyAnswers();
    }

    private void RefreshAllAnswsers()
    {
        Sort(allAnswers);
        allAnswersScroll.initWithData(allAnswers);
    }

    private void RefreshMyAnswers()
    {
        Sort(myAnswers);
        myAnswersScroll.initWithData(myAnswers);
    }

    private void UpdateUI()
    {
        emptyGo.SetActive(myAnswers.Count == 0 && myAnswersScroll.gameObject.activeSelf);
    }

    private void OnAnswerAdded(PKAnswer answer)
    {
        allAnswers.Add(ToPKAnswerCellData(answer));
        RefreshAllAnswsers();

        if (answer.AnswerUserId == UserManager.Instance.UserId)
        {
            myAnswers.Add(ToPKAnswerCellData(answer));
            RefreshMyAnswers();
        }

        UpdateUI();
    }

    public void UploadCode()
    {
        var helper = new PKAnswerUploadHelper(pk);
        helper.Upload();
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
        pk.onAnswerAdded -= OnAnswerAdded;
    }

    public void OnClickChallenge(PKAnswerCell cell)
    {
        Challenge(cell.pkAnswer);
    }

    private void Challenge(PKAnswer rivalAnswer)
    {
        var helper = new PkChallengeHelper(answerSelectView, pk, rivalAnswer);
        helper.Challenge();
    }

    public void OnClickRecord(PKAnswerCell cell)
    {
        pkRecord.gameObject.SetActive(true);
        pkRecord.SetData(pk, cell.pkAnswer);
    }

    public void ShowAllAnswers()
    {
        allAnswersScroll.gameObject.SetActive(true);
        myAnswersScroll.gameObject.SetActive(false);
        UpdateUI();
    }

    public void ShowMyAnswers()
    {
        allAnswersScroll.gameObject.SetActive(false);
        myAnswersScroll.gameObject.SetActive(true);
        UpdateUI();
    }
}
