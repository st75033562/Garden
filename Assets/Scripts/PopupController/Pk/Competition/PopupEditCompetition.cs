using Google.Protobuf;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupEditCompetition : PopupController
{
    public InputField m_nameInput;
    public DateInputWidget m_startDateWidget;
    public DateInputWidget m_endDateWidget;
    public Button m_okButton;

    public GameObject m_coverGo;
    public UIImageMedia m_coverImage;
    public Button btnAddHonor;
    public AssetBundleSprite honorCover;
    public GameObject contentPanel;

    private ICompetitionService m_service;
    private Competition m_item;

    // non null if trophy changed
    private CourseTrophySetting m_newTrophySetting;

    // non null if cover changed
    private LocalResData m_coverResource;

    public void Initialize(Competition item, ICompetitionService service)
    {
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }

        m_service = service;
        m_item = item;
    }

    protected override void Start()
    {
        base.Start();

        _titleText.text = m_item != null ? "ui_pk_competition_edit".Localize() : "ui_pk_competition_new".Localize();

        m_nameInput.onValueChanged.AddListener(OnNameChanged);
        if (m_item != null)
        {
            UpdateUI(m_item);
        }
        else
        {
            UpdateUI(new Competition {
                startTime = ServerTime.UtcNow,
                duration = TimeSpan.FromDays(1)
            });
        }
    }

    private void UpdateUI(Competition item)
    {
        m_nameInput.text = item.name;
        m_startDateWidget.utcDate = item.startTime;
        m_endDateWidget.utcDate = item.endTime;
        
        bool hasCover = item.coverUrl != string.Empty;
        m_coverGo.SetActive(hasCover);
        if (hasCover)
        {
            m_coverImage.SetImage(item.coverUrl);
        }
        UpdateHonorCover(item.courseTrophySetting);
    }

    private void OnNameChanged(string text)
    {
        UpdateOkButton();
    }

    private void UpdateOkButton()
    {
        m_okButton.interactable = m_nameInput.text != string.Empty;
    }

    public void OnClickNext()
    {
        contentPanel.SetActive(false);
    }

    public void OnClickBack()
    {
        contentPanel.SetActive(true);
    }

    public void OnClickOk()
    {
        if (m_startDateWidget.utcDate >= m_endDateWidget.utcDate)
        {
            PopupManager.Notice("ui_pk_competition_end_time_greater_than_start_time".Localize());
            return;
        }

        UploadCover();
    }

    private void UploadCover()
    {
        if (m_coverResource != null)
        {
            Uploads.UploadMedia(m_coverResource.textureData, m_coverResource.name, false)
                   .Blocking()
                   .Success(CreateOrUpdateCompetition)
                   .Execute();
        }
        else
        {
            CreateOrUpdateCompetition();
        }
    }

    private void CreateOrUpdateCompetition()
    {
        if (m_item != null)
        {
            UpdateCompetition();
        }
        else
        {
            CreateCompetition();
        }
    }

    private void UpdateCompetition()
    {
        int maskId = PopupManager.ShowMask();

        var update = new CompetitionUpdate();
        update.name = m_nameInput.text;
        update.startTime = m_startDateWidget.utcDate;
        update.endTime = m_endDateWidget.utcDate;
        update.trophySetting = m_newTrophySetting;
        if (m_coverResource != null)
        {
            update.coverUrl = m_coverResource.name;
        }

        m_service.UpdateCompetition(m_item, update, res => {
            PopupManager.Close(maskId);
            if (res == Command_Result.CmdNoError)
            {
                m_newTrophySetting = null;
                EventBus.Default.AddEvent(EventId.CompetitionUpdated, m_item);
                OnRightClose();
            }
            else
            {
                PopupManager.Notice("ui_pk_competition_update_failed".Localize());
            }
        });
    }

    private void CreateCompetition()
    {
        int maskId = PopupManager.ShowMask();

        Competition newComp = new Competition();

        newComp.name = m_nameInput.text;
        newComp.creatorId = UserManager.Instance.AccountId;
        newComp.startTime = m_startDateWidget.utcDate;
        newComp.duration = m_endDateWidget.utcDate - m_startDateWidget.utcDate;
        newComp.courseTrophySetting = m_newTrophySetting;

        if (m_coverResource != null)
        {
            newComp.coverUrl = m_coverResource.name;
        }

        m_service.CreateCompetition(newComp, res => {
            PopupManager.Close(maskId);
            if (res == Command_Result.CmdNoError)
            {
                m_item = newComp;
                m_newTrophySetting = null;

                EventBus.Default.AddEvent(EventId.CompetitionCreated, m_item);
                OnRightClose();
            }
            else
            {
                PopupManager.Notice("ui_pk_competition_create_failed".Localize());
            }
        });
    }

    public void OnClickAddCover()
    {
        LocalResOperate.instance.OpenResWindow(LocalResType.IMAGE, res => {
            var tex = new Texture2D(0, 0);
            if (!tex.LoadImage(res.imageData))
            {
                Destroy(tex);
                Debug.LogError("invalid image");
                return;
            }

            if (tex.height > DataAccess.Constants.CompetitionCoverRefHeight)
            {
                TextureScale.Bilinear(tex, DataAccess.Constants.CompetitionCoverRefHeight);
            }

            m_coverResource = LocalResData.Image(tex.EncodeToPNG());
            tex.Apply(false, true);
            m_coverGo.SetActive(true);
            m_coverImage.SetImage(tex);
        });
    }

    public void OnClickAddHonor()
    {
        PopupManager.SetMatchHonor(_titleText.text, (courseRaceType) => {
            PopupSetTrophyData data = new PopupSetTrophyData();
            data.courseRaceType = courseRaceType;
            if (m_newTrophySetting != null)
            {
                data.trophySetting = m_newTrophySetting;
            }
            else if (m_item != null && m_item.courseTrophySetting != null)
            {
                data.trophySetting = new CourseTrophySetting(m_item.courseTrophySetting);
            }

            PopupManager.SetTrophy(_titleText.text, data, () => {
                m_newTrophySetting = data.trophySetting;
                UpdateHonorCover(data.trophySetting);

                UpdateOkButton();
            });
        });
    }

    void UpdateHonorCover(CourseTrophySetting setting)
    {
        if (setting != null)
        {
            btnAddHonor.interactable = false;
            honorCover.gameObject.SetActive(true);

            var trophyResultId = setting.goldTrophy.trophyResultId;
            var trophyResultData = TrophyResultData.GeTrophyData(trophyResultId);
            honorCover.SetAsset(trophyResultData.previewBundleName, trophyResultData.previewAssetName);
        }
        else
        {
            btnAddHonor.interactable = true;
            honorCover.gameObject.SetActive(false);
        }
        UpdateOkButton();
    }

    public void OnClickDeleteTrophy()
    {
        if (m_item != null && m_item.courseTrophySetting != null)
        {
            int popuoId = PopupManager.ShowMask();
            m_service.DeleteTrophySettings(m_item.id, res => {
                PopupManager.Close(popuoId);
                if (res == Command_Result.CmdNoError)
                {
                    m_item.courseTrophySetting = null;
                    m_newTrophySetting = null;
                    UpdateHonorCover(null);
                }
                else
                {
                    PopupManager.Notice(res.Localize());
                }
            });
        }
        else
        {
            m_newTrophySetting = null;
            UpdateHonorCover(null);
        }
    }
}
