using DataAccess;
using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : ManagerBase {
	public Canvas m_Canvas;
	public UILogin m_LoginUI;
	public UIRegister m_RegistUI;
	public UILobbyMyInfo m_PlayInfo;
	public UILobbyAvatar m_Avatar;
	public UILobbyDetailInfo m_detail;
	public UIChangePassword m_ChangePasswordUI;
	public GameObject m_bluetoothMark;
	public Text m_textName;
	public Text m_textId;
	public Image m_AvatarImage;
    public Text m_ScriptLanguageText;
    public Text m_CoinText;

	public GameObject m_projectDownloadProgress;
	public ProgressBar m_downloadProgressBar;
	public Text m_downloadProgressText;
	public Text m_versionText;
    public GameObject honorCertificate;
    public GameObject trophyGo;
    public Text certificateCount;
    public Text trophyCount;
    public GameObject gbGo;
    public GameObject classGo;

    private bool m_gotoNextScene;

    public enum eHideEditorUI
    {
        YES_HIDE = 0,
        NO_HIDE = 1,
        UNKNOWN = 2,
    }
    private eHideEditorUI curHideEditorWhenRun = eHideEditorUI.UNKNOWN;
    public eHideEditorUI CurrentHideEditorWhenRun
    {
        get { return curHideEditorWhenRun; }
    }

    public void Restore(LobbySceneSaveState state)
    {
        if (state == null)
        {
            throw new ArgumentNullException();
        }

        if (ArePrerequisitesMet())
        {
            if (state.initialPopup == LobbySceneSaveState.Popup.Project)
            {
                ShowProjectView(state.initialDir);
            }
            else if (state.initialPopup == LobbySceneSaveState.Popup.Gameboard)
            {
                ShowSelectGameboard(state.initialDir, state.initialGameboardThemeId);
            }
        }
    }

	// Use this for initialization
    void Start()
    {
        gbGo.SetActive(false);
        classGo.SetActive(false);

        ResetMask();

		if(0 != UserManager.Instance.UserId) {
            ShowUserInfo();
        } else {
			ShowLogin();
        }
		m_versionText.text = string.Format("v{0}", Application.version + (AppConfig.TestServer ? "*" : ""));

		UserManager.Instance.appRunModel = AppRunModel.Normal;
		UserManager.Instance.isSimulationModel = false;
#if !UNITY_STANDALONE_WIN
		OnBLEStateChanged(Robomation.RobotManager.instance.isConnectionEnabled);
		Robomation.RobotManager.instance.onConnectionEnabled += OnBLEStateChanged;
#endif

        Preference.onScriptLanguageChanged += UpdateScriptLanguageText;
        UserManager.Instance.onAvatarIdChanged += UpdateAvatar;

        NodeTemplateCache.Instance.LoadNodeInitState();
        

    }

    void OnDestroy()
    {
        Preference.onScriptLanguageChanged -= UpdateScriptLanguageText;
#if !UNITY_STANDALONE_WIN
		Robomation.RobotManager.instance.onConnectionEnabled -= OnBLEStateChanged;
#endif
        UserManager.Instance.onAvatarIdChanged -= UpdateAvatar;
    }

    void UpdateScriptLanguageText()
    {
        switch (Preference.scriptLanguage)
        {
        case ScriptLanguage.Visual:
            m_ScriptLanguageText.text = "ui_text_graphical".Localize();
            break;
        case ScriptLanguage.Python:
            m_ScriptLanguageText.text = "Python";
            break;
        }
        // LanguageText.gameObject.SetActive(UserManager.Instance.UserId != 0);
    }

    void OnBLEStateChanged(bool state)
    {
        m_bluetoothMark.SetActive(!state);
    }

    public void ShowRegist()
    {
        m_RegistUI.SetActive(true);
    }

    public void ShowLogin()
    {
        m_LoginUI.SetActive(true);
    }

	public void LoginSuccess() {

        Preference.SetUserId(UserManager.Instance.UserId);
		VideoRecorder.SetUserId(UserManager.Instance.UserId);
		CodeProjectRepository.instance.initialize("Projects", UserManager.Instance.UserId);
		GameboardRepository.instance.initialize("Gameboards", UserManager.Instance.UserId);
//		PythonRepository.instance.initialize("Pythons", UserManager.Instance.UserId);
        CodeSession.Init(string.Format("{0}/{1}/CodeSession", Application.persistentDataPath, UserManager.Instance.UserId));

		if(!UserManager.Instance.IsAccountExpired) {
			RequestAllClass(false);
			StartCoroutine(InitializeOnLogin());
		}
        ShowUserInfo();
        UpdateScriptLanguageText();

        HonorNotificationService.instance.Listen();
        HonorNotificationService.instance.GetNewHonors();
    }


	void ShowUserInfo() {
        if(UserManager.Instance.IsAdminOrTeacher) {
            gbGo.SetActive(true);
            classGo.SetActive(true);
        } else {
            gbGo.SetActive(false);
            classGo.SetActive(false);
        }
        certificateCount.text = HonorWallData.instance.GetCertificates().Count.ToString();
        trophyCount.text = HonorWallData.instance.GetTrophys().Count.ToString();

        m_textName.text = UserManager.Instance.Nickname.EllipsisChar();
		m_textId.text = "ui_text_user_info_id".Localize() + UserManager.Instance.UserId.ToString();
        m_CoinText.text = UserManager.Instance.Coin.ToString();
        UpdateAvatar();
        m_PlayInfo.UpdateNick();
    }

    private void UpdateAvatar()
    {
		m_AvatarImage.sprite = UserIconResource.GetUserIcon(UserManager.Instance.AvatarID);
    }

	IEnumerator InitializeOnLogin()
	{
        yield return SyncProjects("ui_sync_gameboard", new ProjectSynchronizer(GameboardRepository.instance, GetCatalogType.GAME_BOARD_V2));
   //     yield return SyncProjects("ui_sync_robot_code", new ProjectSynchronizer(CodeProjectRepository.instance, GetCatalogType.SELF_PROJECT_V2));
//#if UNITY_EDITOR || UNITY_STANDALONE
//        yield return SyncProjects("ui_sync_python_code", new ProjectSynchronizer(PythonRepository.instance, GetCatalogType.PYTHON));
//#endif

        //if (!Application.isMobilePlatform)
        //{
        //    PythonScriptAutoUploader.instance.Start();
        //}

        yield return UserManager.Instance.userSettings.SyncAllSettings();

        var session = CodeSession.Load();
        if (session != null)
        {
            PopupManager.YesNo("ui_enter_temp_code_notice".Localize(),
                () => {
                    CodeSession.Delete();
                    SceneDirector.Push("Main", CodeSceneArgs.FromSession(session));
                },
                () => {
                    CodeSession.Delete();
                });
        }
        EventBus.Default.AddEvent(EventId.UserLoggedIn);
	}

    IEnumerator SyncProjects(string hint, ProjectSynchronizer synchronizer)
    {
		bool done = false;
		m_projectDownloadProgress.SetActive(true);
		m_downloadProgressBar.progress = 0;
		m_downloadProgressBar.hint = hint.Localize();
		m_downloadProgressText.text = string.Empty;

        synchronizer.onDownloadFinished += () => {
            if (!synchronizer.finished)
            {
                // show download error
                PopupManager.YesNo("ui_project_download_failed".Localize(),
                    synchronizer.RetryFailedDownloads,
                    () => {
                        done = true;
                    });
            }
            else
            {
                done = true;
            }
        };

        synchronizer.onGetFileListFailed += () => {
            PopupManager.YesNo("ui_project_sync_failed".Localize(),
                () => {
                    synchronizer.Synchronize();
                },
                () => {
                    done = true;
                });
        };

		synchronizer.onProgressChanged += () => {
			m_downloadProgressBar.progress = synchronizer.successfulSyncs / (float)synchronizer.totalFilesToSync;
			m_downloadProgressText.text = string.Format("{0}/{1}", synchronizer.successfulSyncs.ToString(), synchronizer.totalFilesToSync.ToString());
		};

		synchronizer.Synchronize();

        while (!done)
        {
            yield return null;
        }

        if (synchronizer.finished)
        {
            // visual feedback in case progress bar was not updated
            m_downloadProgressBar.progress = 1.0f;
            yield return new WaitForSeconds(0.15f);
        }

		m_projectDownloadProgress.SetActive(false);
	}

	public void Logout()
	{
		m_PlayInfo.SetActive(false);
		NetworkSessionController.instance.Logout(false);
        UpdateScriptLanguageText();
    }

	public void ShowAvatar(UIAvatarWorkMode mode, Action<int> callBack = null)
	{
		m_Avatar.SetActive(true, mode, callBack);
	}

	public void ClickProgram()
	{
		if (!CheckPrerequisites())
		{
			return;
		}

		if (Preference.scriptLanguage == ScriptLanguage.Visual)
        {
            ShowProjectView(null);
        }
        else
        {
            PopupManager.PythonProjectView();
        }
	}

    private void ShowProjectView(IRepositoryPath initialDir)
    {
        PopupManager.ProjectView(path => {
            SceneDirector.Push("Main",
                               CodeSceneArgs.FromPath(path.ToString()),
                               new LobbySceneSaveState {
                                   initialPopup = LobbySceneSaveState.Popup.Project,
                                   initialDir = path.parent
                               });
        }, initialDir: initialDir);
    }

	public void ClickRobot()
	{
        if (!CheckPrerequisites())
        {
			return;
		}
		SceneDirector.Push("RobotManage");
	}

	public void ClickClass()
	{
		if (!CheckPrerequisites())
		{
			return;
		}

		//RequestMail(); ytx 不在这个位置请求
		if (UserManager.Instance.IsTeacher)
		{
			//ShowMask();
			RequestAllClass(true);
			//SceneDirector.Push("TeacherManage");
		}
		else
		{
			SceneDirector.Push("LessonCenter");
			//   UnityEngine.SceneManagement.SceneManager.LoadScene("MyClassScene" );
		}
	}

	void RequestAllClass(bool NextScene)
	{
		m_gotoNextScene = NextScene;
		CMD_Get_Classinfo_r_Parameters tClassRequest = new CMD_Get_Classinfo_r_Parameters();
		for (int i = 0; i < UserManager.Instance.ClassList.Count; ++i)
		{
			tClassRequest.ReqClassList.Add(UserManager.Instance.ClassList[i].m_ID);
		}
		SocketManager.instance.send(Command_ID.CmdGetClassinfoR, tClassRequest.ToByteString(), AllClassCallBack);
		ShowMask();
	}

	private void AllClassCallBack(Command_Result res, ByteString content)
	{
		CloseMask();
		if (res == Command_Result.CmdNoError)
		{
			CMD_Get_Classinfo_a_Parameters tRt = CMD_Get_Classinfo_a_Parameters.Parser.ParseFrom(content);
			if (null != tRt.ClassInfoList)
			{
				for (int i = 0; i < tRt.ClassInfoList.Count; ++i)
				{
					A8_Class_Info tCurNetData = tRt.ClassInfoList[i];
					ClassInfo tCurClass = UserManager.Instance.GetClass(tCurNetData.ClassId);
					if (null != tCurClass)
					{
						tCurClass.UpdateInfo(tCurNetData);
					}
				}

				if (m_gotoNextScene)
				{
					SceneDirector.Push("LessonCenter");
				}
				//RequestAllTask();
			}
		}
		else
		{
			RequestErrorCode(res);
		}
	}

    public void ClickIcon(bool state)
    {
        m_PlayInfo.SetActive(state);
        curHideEditorWhenRun = eHideEditorUI.YES_HIDE;
        UIGameboard._curHideEditWhenRun = curHideEditorWhenRun;
    }

	public void ClickShare()
	{
        if (!CheckPrerequisites())
        {
			return;
		}
		SceneDirector.Push("ShareScene");
	}

	public void ClickPK()
	{
        if (!CheckPrerequisites())
        {
			return;
		}
        if (UserManager.Instance.IsAdmin)
        {
            GetComponent<Canvas>().enabled = false;
            PopupManager.AdminCompetition(() => {
                if (GetComponent<Canvas>() != null)
                    GetComponent<Canvas>().enabled = true;
            });
        }
        else
        {
            NodeTemplateCache.Instance.ShowBlockUI = false;
            GetComponent<Canvas>().enabled = false;
            PopupManager.StudentCompetition(() => {
                if (GetComponent<Canvas>() != null)
                    GetComponent<Canvas>().enabled = true;
            });
        }
    }

    public void OnClickMultiPlayer()
    {
        GetComponent<Canvas>().enabled = false;
        //PopupManager.Exercises(() =>
        //{
        //    if (GetComponent<Canvas>() != null)
        //        GetComponent<Canvas>().enabled = true;
        //});
        PopupManager.GameBoardBank(null, () =>
        {
            if (GetComponent<Canvas>() != null)
                GetComponent<Canvas>().enabled = true;
        });

    }

    public void ClickGameboard()
	{
        NodeTemplateCache.Instance.ShowBlockUI = UserManager.Instance.IsAdminOrTeacher;
        curHideEditorWhenRun = eHideEditorUI.NO_HIDE;
        UIGameboard._curHideEditWhenRun = curHideEditorWhenRun;

        if (!CheckPrerequisites())
		{
			return;
		}
        if(!UserManager.Instance.IsGameboardUser) {
            PopupManager.ActivationCode(PopupActivation.Type.GameBoard);
            return;
        }

        ShowSelectGameboard(null, 0);
	}

    private void ShowSelectGameboard(IRepositoryPath initialDir, int initialThemeId)
    {
		m_Canvas.enabled = false;
		GameboardUtils.SelectGameboard(
            (result) => {
				m_Canvas.enabled = true;

                var saveState = new LobbySceneSaveState {
                    initialPopup = LobbySceneSaveState.Popup.Gameboard,
                    initialDir = result.path.parent,
                };
                if (result.path.name == "")
                {
                    saveState.initialGameboardThemeId = GameboardTemplateData.Get(result.templateId).themeId;
                }

				SceneDirector.Push("GameboardScene",
                                   new GameboardScenePayload(result.templateId, result.path),
                                   saveState);
			},
			() => {
				m_Canvas.enabled = true;
			},
            initialDir: initialDir,
            initialThemeId: initialThemeId);
    }

	public void ClickCommunity()
	{
		ShowMaskTips("ui_function_not_available".Localize());
	}

	/// <summary>
	/// return false if any prerequisites are not satisfied
	/// </summary>
	/// <returns></returns>
	private bool CheckPrerequisites()
	{
		if (!UserManager.Instance.IsLoggedIn)
		{
			ShowMaskTips("login_required".Localize());
			return false;
		}
		if (UserManager.Instance.IsAccountExpired)
		{
            PopupManager.ActivationAccount((expertTime)=> {
                Logout();
                ShowLogin();
            });
            return false;
		}

		return true;
	}

    private bool ArePrerequisitesMet()
    {
        return UserManager.Instance.IsLoggedIn &&
               !UserManager.Instance.IsAccountExpired;
    }

	public void ShowChangePassword()
	{
		m_ChangePasswordUI.Show(UIChangePassword.Mode.ChangePassword);
	}

	public void ShowForgotPassword()
	{
		m_ChangePasswordUI.Show(UIChangePassword.Mode.ForgotPassword);
	}

    public void OnClickHonour(int type) {
        var configure = new PopupHonorConfigure();
        configure.openType = (PopupHonour.Mode)type;
        PopupManager.ShowHonour(configure);
    }
}
