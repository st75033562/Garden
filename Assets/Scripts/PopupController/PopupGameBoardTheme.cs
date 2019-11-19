using DataAccess;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameboardThemePayload
{
    public Action<int> callback;
    public int initialThemeId;
}

public class PopupGameBoardTheme : PopupController {
    [SerializeField]
    private ScrollLoopController templateScroll;

    [SerializeField]
    private ScrollLoopController themeScroll;

    [SerializeField]
    private Text backButtonText;

    private Action<int> callBack;
    private GameBoardTemplateCell selectTemplateCell;
    private GameboardThemeData currentTheme;
    private bool initialized;
    private const int tutleId = 13;

    // Use this for initialization
    protected override void Start () {
        var config = (PopupGameboardThemePayload)payload;
        callBack = config.callback;

        themeScroll.context = this;
        templateScroll.context = this;
        ShowThemeView();

        var initialTheme = GameboardThemeData.Get(config.initialThemeId);

        if (initialTheme != null)
        {
            currentTheme = initialTheme;
            ShowTemplateView();
        }
    }

    private void ShowThemeView()
    {
        backButtonText.text = "ui_gameboard_select_theme".Localize();

        themeScroll.gameObject.SetActive(true);
        templateScroll.gameObject.SetActive(false);
        if (!initialized)
        {
            initialized = true;
            themeScroll.initWithData(GameboardThemeData.Data.Where(x => x.enabled && x.id >= tutleId).ToList());
        }
    }

    private void ShowTemplateView()
    {
        backButtonText.text = "ui_gameboard_select_template".Localize();

        themeScroll.gameObject.SetActive(false);
        templateScroll.gameObject.SetActive(true);
        templateScroll.initWithData(GameboardTemplateData.Data.Where(x => x.themeId == currentTheme.id && x.enabled).ToList());
    }

    public void OnClickTemplateCell(GameBoardTemplateCell cell) {
        selectTemplateCell = cell;
        if(callBack != null)
            callBack(selectTemplateCell.gameBoardTemplateData.id);
        OnCloseButton();
    }

    public void OnClickThemeCell(GameBoardThemeCell cell) {
        currentTheme = cell.gameBoardThemeData;
        ShowTemplateView();
    }

    protected override void OnBackPressed()
    {
        OnClickBack();
    }

    public void OnClickBack() {
        if (currentTheme != null) {
            currentTheme = null;
            ShowThemeView();
        } else {
            Close();
        }
    }

    public void OnClickOk() {
        if(callBack != null)
            callBack(selectTemplateCell.gameBoardTemplateData.id);
        OnCloseButton();
    }
}
