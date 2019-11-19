using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyDetailInfo : MonoBehaviour
{
	public LobbyManager m_Manager;
	public Text m_ID;
	public Text m_NickName;
    public Text m_LobbyNickName;
    public Text m_Class;
	public Text m_Honor;
    public Text m_ExpireTime;
	public Image m_AvatarIcon;
    public GameObject changePwdButton;

	// Use this for initialization
	void OnEnable() {
		EventBus.Default.AddListener(EventId.UpdateAvatar, UpdateAvatar);
        SetActive(true);
        LocalizationManager.instance.onLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
		EventBus.Default.RemoveListener(EventId.UpdateAvatar, UpdateAvatar);
        LocalizationManager.instance.onLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        UpdateExpireTime();
    }

    public void SetActive(bool show)
	{
        var classes = UserManager.Instance.ClassList
            .Where(x => x.languageType == Preference.scriptLanguage
                    && x.m_ClassStatus == ClassInfo.Status.Attend_Status)
            .Take(3)
            .Select(x => x.m_Name)
            .ToArray();

        m_Class.text = string.Join("\n", classes);

        if(show)
		{
			m_ID.text = UserManager.Instance.AccountName.ToString();
			m_NickName.text = UserManager.Instance.Nickname.EllipsisChar();

		    //	m_Honor.text = "no data";
			UpdateAvatar();
            UpdateExpireTime();
        }
	}

    private void UpdateExpireTime()
    {
        if (UserManager.Instance.AccountExpireTimeUTC != null)
        {
            var expireTime = UserManager.Instance.AccountExpireTimeUTC.Value.ToLocalTime();
            m_ExpireTime.text = expireTime.ToString("g", CultureInfo.CurrentCulture);
            m_ExpireTime.enabled = true;
        }
        else
        {
            m_ExpireTime.enabled = false;
        }
    }

	public void Logout()
	{
        if (PythonScriptAutoUploader.instance.isUploading)
        {
            PopupManager.YesNo("ui_warning_logout_lose_python_changes".Localize(), DoLogout);
        }
        else
        {
            DoLogout();
        }
	}

    private void DoLogout()
    {
		m_Manager.Logout();
        m_Manager.ShowLogin();
    }


    public void OnClickChangePassword()
    {
        m_Manager.ShowChangePassword();
    }

	public void SelectAvatar()
	{
		m_Manager.ShowAvatar(UIAvatarWorkMode.Change_Enum);
	}

	public void UpdateAvatar(object param = null)
	{
		m_AvatarIcon.sprite = UserIconResource.GetUserIcon(UserManager.Instance.AvatarID);
	}

    public void OnClickModifyNickName() {
        PopupManager.ModifyName(m_NickName.text, (str) => {
            UserManager.Instance.Nickname = str.EllipsisChar();
            m_NickName.text = UserManager.Instance.Nickname;
            m_LobbyNickName.text = UserManager.Instance.Nickname;
        });    
    }

    public void OnClickModifyAccount() {
        PopupManager.ModifyAccount();
    }

    public void OnClickModifyPassWord() {
        PopupManager.ModifyPassWord();
    }
}
