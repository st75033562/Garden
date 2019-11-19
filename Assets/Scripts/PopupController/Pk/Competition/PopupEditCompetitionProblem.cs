using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupEditCompetitionProblem : PopupController, IPopupSelectAttachmentDelegate
{
    public Text m_name;
    public Text m_description;
    public Text m_gameMode;
    public ScrollLoopController scroll;
    public Toggle m_submitTog;
    public Toggle m_playTog;
    public GameObject togGroup;
    public Text textMode;

    public InputField m_nameInput;
    public InputField m_descriptionInput;
    public Text m_gbNamePlaceholderText;

    public Text m_gameboardNameText;
    public Button m_nextButton;
    public Button m_editor;

    private Competition m_competition;
    private CompetitionProblem m_problem;
    private ICompetitionService m_service;

    private PopupSelectAttachment m_attachmentPopup;
    private IRepositoryPath m_gameboardPath; // non null if gameboard changed
    private Queue<AttachmentUpdate> m_attachmentUpdates;
    private Queue<AttachData> m_attachments = new Queue<AttachData>();
    private int m_maskId;


    private List<AttachData> attachDatas = new List<AttachData>();
    private uint periodType = 0;
    private struct AttachmentUpdate
    {
        public enum Action
        {
            Add,
            Remove
        }
        public CompetitionItem item;
        public Action action;

        public static AttachmentUpdate Add(CompetitionItem item)
        {
            return new AttachmentUpdate {
                action = Action.Add,
                item = item
            };
        }

        public static AttachmentUpdate Remove(CompetitionItem item)
        {
            return new AttachmentUpdate {
                action = Action.Remove,
                item = item
            };
        }
    }

    public void Initialize(Competition competition, CompetitionProblem problem, ICompetitionService service)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }
        if (problem != null && problem.competition != competition)
        {
            throw new ArgumentException("problem not belong to competition");
        }

        m_competition = competition;
        m_problem = problem;
        m_service = service;
    }

    protected override void Start()
    {
        base.Start();

        _titleText.text = m_problem != null 
            ? "ui_pk_competition_problem_edit".Localize() 
            : "ui_pk_competition_problem_new".Localize();
        m_nameInput.onValueChanged.AddListener(delegate { UpdateNextButton(); });
        m_descriptionInput.onValueChanged.AddListener(delegate { UpdateNextButton(); });

        if (m_problem != null)
        {
            AttachData data = new AttachData();
            data.itemId = m_problem.gameboardItem.id;
            data.type = AttachData.Type.Gameboard;
            data.webProgramPath = m_problem.gameboardItem.downloadPath;
            data.programNickName = m_problem.gameboardItem.name;
            attachDatas.Add(data);

            IEnumerable<AttachData> resources = CompetitionUtils.GetAttachmentAttachData(m_problem);
            attachDatas.AddRange(resources);

            m_nameInput.text = m_problem.name;
            m_descriptionInput.text = m_problem.description;
            m_gameboardNameText.text = m_problem.gameboardItem != null ? m_problem.gameboardItem.name : string.Empty;
            SetGameboardName(m_problem.gameboardItem.name);
            periodType = m_problem.periodType;
            ChangeMode(false);
        }
        else
        {
            periodType = 1;
            SetGameboardName(string.Empty);
            ChangeMode(true);
        }
        UpdateNextButton();
    }

    public void OnClickChange(bool isEditor) {
        ChangeMode(isEditor);
    }

    public void ChangeMode(bool isEditor, bool updateText = true) {
        List<AddAttachmentCellData> attachCellDatas = new List<AddAttachmentCellData>();
        foreach (var cell in attachDatas)
        {
            if (cell != null && cell.state != AttachData.State.Delete) {
                attachCellDatas.Add(new AddAttachmentCellData(cell, attachDatas));
            }
        }
        
        if (isEditor)
        {
            m_editor.gameObject.SetActive(false);
            m_nextButton.gameObject.SetActive(true);
            m_nameInput.gameObject.SetActive(true);
    //        m_descriptionInput.gameObject.SetActive(true);
            togGroup.SetActive(true);
            textMode.gameObject.SetActive(false);
            m_name.gameObject.SetActive(false);
            m_description.gameObject.SetActive(false);
            m_gameMode.gameObject.SetActive(false);
            if (periodType == 0) {
                m_submitTog.isOn = true;
                m_playTog.isOn = false;
            }
            else
            {
                m_submitTog.isOn = false;
                m_playTog.isOn = true;
            }

            if (updateText) {
                m_nameInput.text = m_name.text;
                m_descriptionInput.text = m_description.text;
            }

            if (attachCellDatas.Count == 0 || attachCellDatas[attachCellDatas.Count - 1] != null) {
                attachCellDatas.Add(null);
            }
        }
        else
        {
            m_editor.gameObject.SetActive(true);
            m_nextButton.gameObject.SetActive(false);
            m_nameInput.gameObject.SetActive(false);
            m_descriptionInput.gameObject.SetActive(false);
            togGroup.SetActive(false);
            textMode.gameObject.SetActive(true);
            m_name.gameObject.SetActive(true);
            m_description.gameObject.SetActive(true);
            m_gameMode.gameObject.SetActive(true);
            if (updateText)
            {
                m_name.text = m_nameInput.text;
                m_description.text = m_descriptionInput.text;
            }

            if (attachCellDatas.Count > 0 && attachCellDatas[attachCellDatas.Count - 1] == null)
            {
                attachCellDatas.RemoveAt(attachCellDatas.Count - 1);
            }
            if (periodType == 0)
            {
                textMode.text = "ui_problem_solving".Localize();
            }
            else
            {
                textMode.text = "ui_gb_m_game".Localize();
            }
            
        }
        scroll.initWithData(attachCellDatas);
    }

    private void SetGameboardName(string name)
    {
        m_gbNamePlaceholderText.enabled = name == string.Empty;
        m_gameboardNameText.text = name;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_attachmentPopup)
        {
            m_attachmentPopup.Close();
        }
        FinishUpdate();
    }

    public void OnClickSelectGameboard()
    {
        PopupManager.GameBoardSelect(new PopupGameBoardSelect.ConfigureParameter {
            selectCallBack = path => {
                m_gameboardPath = path;
                SetGameboardName(path.name);
                UpdateNextButton();
            }
        });
    }

    public void OnClickNext()
    {
        if (m_attachmentPopup == null)
        {
            string title;
            IEnumerable<LocalResData> resources;
            if (m_problem == null)
            {
                title =  "ui_pk_competition_problem_new".Localize();
                resources = null;
            }
            else
            {
                title =  "ui_pk_competition_problem_edit".Localize();
                resources = CompetitionUtils.GetAttachmentResources(m_problem);
            }
            m_attachmentPopup = PopupManager.SelectAttachment(title, resources, this);
        }
        else
        {
            m_attachmentPopup.Show(true);
        }
    }

    private void UpdateNextButton()
    {
        var gameBoard = attachDatas.Find(x=> { return x != null && x.state != AttachData.State.Delete && x.type == AttachData.Type.Gameboard; });
        m_nextButton.interactable = m_nameInput.text != "" &&
                                    gameBoard != null;
    }

    void IPopupSelectAttachmentDelegate.OnConfirm(Action uploadAttachment)
    {
        uploadAttachment();
    }

    void IPopupSelectAttachmentDelegate.OnUploadFinished(IEnumerable<LocalResData> attachments)
    {
        m_attachmentPopup.Close();
        m_attachmentPopup = null;

        CalculateAttachmentUpdates(attachments);
        AddOrUpdateProblem();
    }

    private void CalculateAttachmentUpdates(IEnumerable<LocalResData> attachments)
    {
        var otherItems = attachments.Select(x => {
            var itemType = CompetitionUtils.ToItemType(x.resType);
            return new CompetitionItem(itemType) {
                url = x.name
            };
        });

        var existingItems = m_problem != null ? m_problem.attachments : Enumerable.Empty<CompetitionItem>();
        var result = Utils.Compare(existingItems, otherItems, (a, b) => {
            return a.type == b.type &&
                   a.url == b.url;
        }, false);

        var deletedItems = result.left.Select(x => AttachmentUpdate.Remove(x));
        var newItems = result.right.Select(x => AttachmentUpdate.Add(x));
        m_attachmentUpdates = new Queue<AttachmentUpdate>(newItems.Concat(deletedItems));
    }

    private void AddOrUpdateProblem()
    {
        m_maskId = PopupManager.ShowMask();
        if (m_problem != null)
        {
            var update = new CompetitionProblemUpdate();
            update.name = m_nameInput.text;
            update.description = m_descriptionInput.text;
            m_problem.periodType = periodType;

            m_service.UpdateProblem(m_problem, update, res => {
                if (res == Command_Result.CmdNoError)
                {
                    UploadItems();
                }
                else
                {
                    PopupManager.Notice("ui_pk_competition_problem_update_failed".Localize());
                    FinishUpdate();
                }
            });
        }
        else
        {
            var newProblem = new CompetitionProblem();
            newProblem.name = m_nameInput.text;
            newProblem.description = m_descriptionInput.text;
            newProblem.periodType = periodType;

            m_service.CreateProblem(m_competition, newProblem, res => {
                if (res == Command_Result.CmdNoError)
                {
                    m_problem = newProblem;
                    UploadItems();
                }
                else
                {
                    PopupManager.Notice("ui_pk_competition_problem_create_failed".Localize());
                    FinishUpdate();
                }
            });
        }
    }

    private void UploadItems()
    {
        DelGameboard();
    }

    private void DelGameboard()
    {
        var gameBoard = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard && x.state == AttachData.State.Delete && x.itemId != 0; });
        if (gameBoard != null)
        {
            CompetitionItem compItem = new CompetitionItem(CompetitionItem.Type.Gb);
            compItem.id = gameBoard.itemId;
            compItem.problem = m_problem;
            m_service.DeleteItem(compItem, (result) =>
            {
                UploadGameboard();
            });
        }
        else {
            UploadGameboard();
        }
    }

    private void UploadGameboard()
    {
        try
        {
            var gameBoard = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard && x.state != AttachData.State.Delete && x.itemId == 0; });
            if (gameBoard != null)
            {
                var gameboardItem = new CompetitionGameboardItem();
                gameboardItem.name = gameBoard.programNickName;

                m_service.AddGameboard(m_problem, gameboardItem, gameBoard.programPath, res =>
                {
                    if (res == Command_Result.CmdNoError)
                    {
                        m_gameboardPath = null;
                        ResetRes();
                        UploadAttachments();
                    }
                    else
                    {
                        PopupManager.Notice("ui_pk_competition_problem_create_failed".Localize());
                        FinishUpdate();
                    }
                });
            }
            else {
                ResetRes();
                UploadAttachments();
            }
        }
        catch (IOException e)
        {
            Debug.LogError(e);
            FinishUpdate();
        }
    }

    void ResetRes() {
        var ress = attachDatas.FindAll(x => { return x != null && x.type == AttachData.Type.Res; });
        foreach (var _res in ress)
        {
            if (_res.itemId == 0 && _res.state == AttachData.State.Delete)
            {
                continue;
            }

            AttachData _attachData = new AttachData();
            _attachData.resData = _res.resData;
            _attachData.state = AttachData.State.Delete;
            _attachData.itemId = _res.itemId;
            m_attachments.Enqueue(_attachData);

            m_attachments.Enqueue(_res);
        }
    }

    private void UpdateGameboardItem(CompetitionGameboardItem item)
    {
        item.name = m_gameboardNameText.text;
    }


    private void UploadAttachments()
    {
        if (m_attachments.Count > 0)
        {
            var res = m_attachments.Peek();
            CompetitionItem.Type type;
            if (res.resData.resType == ResType.Image)
            {
                type = CompetitionItem.Type.Image;
            }
            else if (res.resData.resType == ResType.Video)
            {
                type = CompetitionItem.Type.Video;
            }
            else
            {
                type = CompetitionItem.Type.Doc;
            }
            CompetitionItem compItem = new CompetitionItem(type);
            compItem.url = res.resData.name;
            compItem.id = res.itemId;
            compItem.nickName = res.resData.nickName;
            if (res.state == AttachData.State.Delete)
            {
                compItem.problem = m_problem;
                m_service.DeleteItem(compItem, CheckUploadAttachmentResult);
            }
            else
            {
                m_service.AddItem(m_problem, compItem, CheckUploadAttachmentResult);
            }
        }
        else
        {
            FinishUpdate();
            Close();
        }
    }

    private void CheckUploadAttachmentResult(Command_Result res)
    {
        if (res == Command_Result.CmdNoError)
        {
            m_attachments.Dequeue();
            UploadAttachments();
        }
        else
        {
            PopupManager.YesNo(
                "ui_pk_competition_update_attach_error".Localize(),
                UploadAttachments,
                () => {
                    m_attachments.Clear();
                    FinishUpdate();
                },
                acceptText: "ui_retry".Localize());
        }
    }

    private void FinishUpdate()
    {
        PopupManager.Close(m_maskId);

        if (m_problem != null)
        {
            EventBus.Default.AddEvent(EventId.CompetitionProblemUpdated, m_problem);
        }
    }

    public void OnClickAttachment()
    {
        PopupManager.AttachmentManager(attachDatas, null, () => {
            ChangeMode(true, false);
            UpdateNextButton();
        }, programCount: 0, gameboardCount: 1);
    }

    public void OnClickOk() {
        var medias = attachDatas.FindAll(x => { return x != null && x.state != AttachData.State.Delete && x.type == AttachData.Type.Res; });
        UploadMedia(medias);
        ChangeMode(false);
    }

    void UploadMedia(List<AttachData> medias) {
        if (medias.Count == 0) {
            AddOrUpdateProblem();
            return;
        }
        LocalResData data = medias[0].resData;
        medias.RemoveAt(0);
        if (Utils.IsValidUrl(data.name)) {
            UploadMedia(medias);
        }
        else if (data.resType == ResType.Video)
        {
            if (string.IsNullOrEmpty(data.filePath))
            {
                UploadMedia(medias);
            }
            else
            {
                int maskId = PopupManager.ShowMask();
                LoadResource.instance.LoadLocalRes(data.filePath, (www) => {
                    PopupManager.Close(maskId);

                    Uploads.UploadMedia(www.bytes, data.name, true)
                           .Blocking()
                           .Success(() => {
                               UploadMedia(medias);
                           })
                           .Execute();
                });
            }
        }
        else
        {
            if (data.textureData == null)
            {
                UploadMedia(medias);
            }
            else
            {
                Uploads.UploadMedia(data.textureData, data.name, false)
                   .Blocking()
                   .Success(() => {
                       UploadMedia(medias);
                   })
                   .Execute();
            }
            
        }
    }

    public void OnToggle() {
        if (m_submitTog.isOn)
        {
            periodType = 0;
        }
        else {
            periodType = 1;
        }
    }
}
