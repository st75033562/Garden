using AssetBundles;
using Robomation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyMyInfo : MonoBehaviour {
    public LobbyManager m_Manager;
    public Text m_NickName;
    public Image m_AvatarImage;
    public Toggle[] m_languageToggles;
    public Dropdown m_scriptingLangDropDown;
    public GameObject m_scriptLangRoot;
    public Dropdown m_levelDropDown;
    public UILobbyDetailInfo uiLobbyDetailInfo;

    private bool m_started;

    // Use this for initialization
    void OnEnable() {
        m_languageToggles[LocalizationManager.instance.currentLocaleIndex].isOn = true; 
		EventBus.Default.AddListener(EventId.UpdateAvatar, UpdateAvatarIcon);

        InitScriptingLanguageDropdown();
        InitLevelDropdown();

        //现在播放版本，直接设为false
        //m_scriptLangRoot.SetActive(!Application.isMobilePlatform);
        m_scriptLangRoot.SetActive(false);
        m_scriptingLangDropDown.onValueChanged.AddListener(OnScriptingLanguageChanged);
        m_started = true;
    }

    private void OnDisable()
    {
        EventBus.Default.RemoveListener(EventId.UpdateAvatar, UpdateAvatarIcon);
    }

    private void InitScriptingLanguageDropdown() {
        m_scriptingLangDropDown.ClearOptions();

        List<string> list = new List<string>();
        list.Add("ui_text_graphical".Localize());
        list.Add("Python");

        m_scriptingLangDropDown.AddOptions(list);
        m_scriptingLangDropDown.value = (int)Preference.scriptLanguage;
    }

    private void InitLevelDropdown() {
        m_levelDropDown.ClearOptions();
        List<string> list = new List<string>();
        list.Add("setting_level_1".Localize());
        list.Add("setting_level_2".Localize());
        list.Add("setting_level_3".Localize());
        list.Add("setting_level_4".Localize());
        m_levelDropDown.AddOptions(list);
        m_levelDropDown.value = (int)Preference.blockLevel - 1;
    }

    public void SetActive(bool show) {
        if(show) {
            UpdateNick();
            UpdateAvatarIcon();
        }
        gameObject.SetActive(show);
    }

    public void DropdownLevelChange(int value) {
        Preference.blockLevel = (BlockLevel)(value + 1);
    }

    public void OnScriptingLanguageChanged(int value) {
        if(UserManager.Instance.IsAccountExpired) {
            PopupManager.ActivationAccount((expertTime)=> {
                m_Manager.Logout();
                m_Manager.ShowLogin();
            });
            return;
        }

        var newLang = (ScriptLanguage)value;
        if (newLang == ScriptLanguage.Python && !UserManager.Instance.IsPythonUser) {
            // reset to the old option until python is activated
            m_scriptingLangDropDown.value = (int)Preference.scriptLanguage;
            PopupManager.ActivationCode(PopupActivation.Type.Python, () => {
                Preference.scriptLanguage = newLang;
                m_scriptingLangDropDown.value = (int)ScriptLanguage.Python;
            });
        } else {
            Preference.scriptLanguage = newLang;
        }

        RobotManager.instance.uninitialize();
        uiLobbyDetailInfo.SetActive(false);
    }

    public void UpdateNick() {
        m_NickName.text = UserManager.Instance.Nickname.EllipsisChar();
    }

    private void UpdateAvatarIcon(object param = null) {
        m_AvatarImage.sprite = UserIconResource.GetUserIcon(UserManager.Instance.AvatarID);
    }

    public void ToggleToChinese(bool isOn) {
        if (!m_started || !isOn) { return; }

        ChangeLanguage(SystemLanguage.ChineseSimplified);
    }

    public void ToggleToEnglish(bool isOn) {
        if (!m_started || !isOn) { return; }

        ChangeLanguage(SystemLanguage.English);
    }

    private void ChangeLanguage(SystemLanguage lang)
    {
        if (lang == LocalizationManager.instance.language)
        {
            return;
        }

        LocalizationManager.instance.language = lang;
        StartCoroutine(ChangeLanguage());
    }

    private IEnumerator ChangeLanguage()
    {
        int maskId = PopupManager.ShowMask("ui_switching_language".Localize());

        yield return LocalizationManager.instance.loadData();
        Preference.language = LocalizationManager.instance.language;

        while (AssetBundleManager.hasPendingOperations)
        {
            yield return null;
        }
        AssetBundleManager.UnloadVariantBundles();
        AssetBundleManager.UnloadUnusedBundles();
        Initialization.ResetAssetBundleVariants();
        NodeTemplateCache.Instance.Refresh();

        InitScriptingLanguageDropdown();
        InitLevelDropdown();

        PopupManager.Close(maskId);
    }

	public void SelectAvatar()
	{
		m_Manager.ShowAvatar(UIAvatarWorkMode.Change_Enum);
	}
}
