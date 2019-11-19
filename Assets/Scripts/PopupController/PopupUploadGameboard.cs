using System;
using UnityEngine;
using UnityEngine.UI;

public class GameboardUploadInfo
{
    public string gameboardName;
    public string gameboardPath;
    public bool showSourceCode;
    public string battleName;
    public string description;
    public string startPointInfo;
    public bool allowRepeatChallenge;
    public PopupILPeriod.PassModeType passModeType;
    public LocalResData res;
}

public delegate void PopupUploadGameboardHandler(GameboardUploadInfo info);

public class PopupUploadGameboardPayload
{
    public readonly PopupUploadGameboardHandler uploadHandler;
    public Func<Gameboard.Gameboard, bool> gameboardFilter;
   

    public PopupUploadGameboardPayload(PopupUploadGameboardHandler handler, Func<Gameboard.Gameboard, bool> filter)
    {
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }

        uploadHandler = handler;
        gameboardFilter = filter;
    }
}

public class PopupUploadGameboard : PopupController
{
    public InputField m_battleNameInput;
    public InputField m_descInput;
    public InputField m_startPointInput;
    public Text m_gbNameText;
    public Text m_gbNamePlaceholderText;
    public Toggle m_repeatChallengeToggle;
    public Button m_buttonNext;
    public UIImageMedia coverImage;

    private IRepositoryPath m_gameboardPath;

    private PopupILPeriod.PassModeType passModeType = PopupILPeriod.PassModeType.Play;
    private LocalResData res;
    private const int referenceHeight = 512;
    protected override void Start()
    {
        base.Start();

        m_battleNameInput.onValueChanged.AddListener((str)=> {
            CheckNextButton();
        });
        m_descInput.onValueChanged.AddListener((str) => {
            CheckNextButton();
        });
        m_startPointInput.onValueChanged.AddListener((str) => {
            CheckNextButton();
        });

        SetGameboardName(string.Empty);
        CheckNextButton();
    }

    private void SetGameboardName(string name)
    {
        m_gbNamePlaceholderText.enabled = name == string.Empty;
        m_gbNameText.text = name;
    }

    public void OnClickSelectGameboard()
    {
        PopupManager.GameBoardSelect(new PopupGameBoardSelect.ConfigureParameter {
            visibleType = PopupGameBoardSelect.VisibleType.SourceAvailable,
            filter = payload.gameboardFilter,
            selectCallBack = OnSelectGameboard
        });
    }

    public void OnClickNext()
    {
        var error = ProjectNameValidator.Validate(GameboardRepository.instance, battleName);
        if (error != null)
        {
            PopupManager.Notice("ui_upload_gameboard_invalid_name".Localize(error));
            return;
        }

        //PopupManager.TwoBtnDialog("gameboard_upload_notice".Localize(),
        //   "gameboard_upload_yes".Localize(),
        //   () => {
        //       ConfirmUpload(true);
        //   },
        //   "gameboard_upload_no".Localize(),
        //   () => {
        //       ConfirmUpload(false);
        //   });
        ConfirmUpload(false);
    }

    void ConfirmUpload(bool showSourceCode) {
        PopupManager.TwoBtnDialog("ui_publish_gameboard_warning".Localize(),
           "ui_confirm".Localize(),
           () => {
               Upload(showSourceCode);
           },
           "ui_cancel".Localize(),
           null);
    }

    private new PopupUploadGameboardPayload payload
    {
        get { return (PopupUploadGameboardPayload)base.payload;}
    }

    private void Upload(bool showSourceCode)
    {
        payload.uploadHandler(new GameboardUploadInfo {
            gameboardName = m_gbNameText.text,
            gameboardPath = m_gameboardPath.ToString(),
            showSourceCode = showSourceCode,
            battleName = battleName,
            description = m_descInput.text,
            startPointInfo = m_startPointInput.text,
            allowRepeatChallenge = m_repeatChallengeToggle.isOn,
            passModeType = passModeType,
            res = res
        });
    }

    private string battleName
    {
        get { return m_battleNameInput.text.TrimEnd(); }
    }

    private void CheckNextButton()
    {
        m_buttonNext.interactable = battleName != "" &&
                                    m_gbNameText.text != "";
    }

    void OnSelectGameboard(IRepositoryPath path)
    {
        m_gameboardPath = path;
        SetGameboardName(path.name);
        CheckNextButton();
    }

    public void OnToggleMode(int mode) {
        passModeType = (PopupILPeriod.PassModeType)mode;
    }

    public void OnClickAddCover() {
        LocalResOperate.instance.OpenResWindow(LocalResType.IMAGE, (data) => {
            Texture2D texture = new Texture2D(0, 0);
            if(!texture.LoadImage(data.imageData)) {
                Destroy(texture);
                Debug.LogError("failed to load image " + data.path);
                return;
            }

            if(texture.height > referenceHeight) {
                TextureScale.Bilinear(texture, referenceHeight);
            }

            res = LocalResData.Image(texture.EncodeToPNG());
            coverImage.gameObject.SetActive(true);
            coverImage.SetImage(texture);
        });
    }
}
