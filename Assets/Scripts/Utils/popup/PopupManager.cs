using Gameboard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    private static int s_lastPopupId;
    private const int s_kMinSortingOrder = 10;

    private readonly static List<PopupController> popupStack = new List<PopupController>();

    private delegate PopupController PopupFactory(int popupId);
    private struct QueuedPopup : IEquatable<QueuedPopup>
    {
        public int popupId;
        public PopupFactory factory;

        public bool Equals(QueuedPopup popup)
        {
            return popupId == popup.popupId;
        }
    }

    private static PopupController activeQueuedPopup;
    private static readonly LinkedList<QueuedPopup> queuedPopups = new LinkedList<QueuedPopup>();

    private void Awake()
    {
        SceneDirector.onLoadingScene += delegate { ClearQueuedPopups(); };
    }

    public static int popupCount
    {
        get { return popupStack.Count; }
    }

    private static int EnqueuePopup(PopupFactory factory)
    {
        int id = ++s_lastPopupId;
        if (!activeQueuedPopup && queuedPopups.Count == 0)
        {
            activeQueuedPopup = factory(id);
        }
        else
        {
            queuedPopups.AddLast(new QueuedPopup {
                popupId = id,
                factory = factory
            });
        }
        return id;
    }

    public static int Notice(string content,  Action onOk = null, bool modal = true, bool queued = true)
    {
        PopupFactory factory = id => {
            var popup = (PopupNotice)InitPopup("popupNotice", id);
            popup.modal = modal;
            popup.content = content;
            popup.closeAction = onOk;
            return popup;
        };

        return queued ? EnqueuePopup(factory) : factory(-1).Id;
    }

    public static int PublishCourseSucess(string content) {
        PopupController popup = InitPopup("popupPublishCourseSucess");
        popup.content = content;
        return popup.Id;
    }
    public static int GoodJob( Action onClose = null) {
        PopupController popup = InitPopup("popupGoodJob");
        popup.closeAction = onClose;
        return popup.Id;
    }

    public static int ModifyName(string title, Action<string> calllBack) {
        PopupController popup = InitPopup("popupModifyName");
        popup.title = title;
        popup.payload = calllBack;
        return popup.Id;
    }

    public static int ModifyPassWord() {
        PopupController popup = InitPopup("popupModifyPassWord");
        return popup.Id;
    }
    
    public static int ModifyAccount() {
        PopupController popup = InitPopup("popupModifyAccount");
        return popup.Id;
    }

    public static int StrawberryMap(object obj , Action onClose = null) {
        PopupController popup = InitPopup("popupStrawberry");
        popup.closeAction = onClose;
        popup.payload = obj;
        return popup.Id;
    }

    public static int GameBoard(PopupGameboardResultCallback resultCallback, IRepositoryPath initialDir, int initialThemeId, Action onClose) {
        PopupController popup = InitPopup("popupGameBoard");
        popup.closeAction = onClose;
        popup.payload = new PopupGameboardPayload {
            callback = resultCallback,
            initialDir = initialDir,
            initialThemeId = initialThemeId
        };
        return popup.Id;
    }

    public static int GameBoardTheme(Action<int> callback, int initialThemeId = 0, Action onClose = null) {
        PopupController popup = InitPopup("popupGameBoardTheme");
        popup.closeAction = onClose;
        popup.payload = new PopupGameboardThemePayload {
            callback = callback,
            initialThemeId = initialThemeId
        };
        return popup.Id;
    }

    public static int GameBoardShare(Action onClose = null) {
        // TODO: add new popup
        return 0;
    }

    public static int SinglePkLeaderboard(GameBoard gameboard, Action onClose = null)
    {
        PopupController popup = InitPopup("popupSinglePkLeaderboard");
        popup.closeAction = onClose;
        popup.title = gameboard.GbName;
        popup.payload = gameboard;
        return popup.Id;
    }

    public static int ShowMask(string content = null)
    {
        if (string.IsNullOrEmpty(content))
        {
            content = "wait_for_server".Localize();
        }
        PopupController popup = InitPopup("popupMark", ++s_lastPopupId);
        popup.content = content;
        return popup.Id;
    }

    public static int YesNo(string content,
                            Action accept,
                            Action reject = null,
                            string acceptText = null,
                            string rejectText = null,
                            bool modal = true,
                            bool queued = true)
    {
        PopupFactory factory = id => {
            var popup = (UIPopupYesNo)InitPopup("popupYesNo", id);
            popup.modal = modal;
            popup.content = content;
            popup.accetAction = accept;
            popup.rejectAction = reject;
            popup.SetAcceptText(acceptText);
            popup.SetRejectText(rejectText);
            return popup;
        };

        return queued ? EnqueuePopup(factory) : factory(-1).Id;
    }

    public static int TwoBtnDialog(string content,
                                   string leftText, 
                                   Action leftAction, 
                                   string rightText,
                                   Action rightAction,
                                   bool modal = true,
                                   bool showCloseButton = true)
    {
        var popup = (PopupTwoBtnDialog)InitPopup("popupTwoBtnDialog");
        popup.content = content;
        popup.leftText = leftText;
        popup.leftAction = leftAction;
        popup.rightText = rightText;
        popup.rightAction = rightAction;
        popup.ShowCloseButton(showCloseButton);
        return popup.Id;
    }

    public static int CloseCourse( Action rightAction) {
        var popup = InitPopup("popupCloseCourseNotice");
        popup.rightAction = rightAction;
        return popup.Id;
    }

    public static int InputDialog(string title, string content, string inputHint,
                                  DialogInputHandler inputCallback,
                                  DialogInputValidationHandler validator,
                                  string confirmText = null)
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditInputDialog>();
        dialog.Init();
        dialog.Configure(
            new UIEditInputDialogConfig {
                title = title,
                content = content,
                inputHint = inputHint,
                confirmText = confirmText
            },
            new DialogInputCallback(inputCallback),
            validator != null ? new DialogInputValidator(validator) : null);

        return dialog.Id;
    }

    public static PopupGameBoardPlayer GameboardPlayer()
    {
        return (PopupGameBoardPlayer)InitPopup("popupGameBoardPlayer");
    }

    public static int GameboardPlayer(ProjectPath gameboardPath, 
                                      RobotCodeInfo[] robotCodes = null,
                                      Action<PopupILPeriod.PassModeType, Gameboard.GameboardResult> onResultSet = null, 
                                      Action onClose = null,
                                      bool editable = false,
                                      Action<Gameboard.Gameboard> gameboardModifier = null,
                                      Gameboard.GameboardCustomCodeGroups customBindings = null,
                                      bool showEditButton = false,
                                      SaveHandler saveHandler = null,
                                      List<string> relations = null,
                                      bool isWebGb = false,
                                      bool showBottomBts = true,
                                      bool noTopBarMode = false)
    {
        var controller = GameboardPlayer();
        controller.payload = new GameboardPlayerConfig {
            editable = editable,
            gameboardPath = gameboardPath,
            gameboardModifier = gameboardModifier,
            robotCodes = robotCodes,
            customBindings = customBindings,
            onResultSet = onResultSet,
            showEditButton = showEditButton,
            saveHandler = saveHandler,
            relations = relations,
            isWebGb = isWebGb,
            showBottomBts = showBottomBts,
            NoTopBarMode = noTopBarMode
        };
        controller.closeAction = onClose;
        return controller.Id;
    }

    public static int GameBoardSelect(PopupGameBoardSelect.ConfigureParameter configure) {
        PopupController popup = InitPopup("PopupSelectGameBoard");
        popup.payload = configure;
        return popup.Id;
    }

    public static int ActivationCode(PopupActivation.Type type, Action onActivated = null) {
        var configure = new ActiveConfigure {
            type = type,
            activeSucess = (activationA) => {
                UserManager.Instance.Authority = (User_Type)activationA.UserType;
                PopupManager.Notice("ui_text_activate_sucess".Localize());
                
                if (onActivated != null)
                {
                    onActivated();
                }
            }
        };
        return ActivationPopup(configure);
    }

    static int ActivationPopup(ActiveConfigure configure) {
        PopupController popup = InitPopup("PopupActivation");
        popup.payload = configure;
        return popup.Id;
    }
    public static int ActivationAccount(Action<ulong> callBack) {
        var configure = new ActiveConfigure {
            type = PopupActivation.Type.Account,
            activeSucess = (activationA) => {
                if(callBack != null) {
                    callBack(activationA.ExpiredTime);
                }
            }
        };
        return ActivationPopup(configure);
    }

    public static int SelectCertificate(string title, Action<int> selectId) {
        PopupController popup = InitPopup("popupSelectCertificate");
        popup.title = title;
        popup.payload = selectId;
        return popup.Id;
    }

    public static int ShowHonour(PopupHonorConfigure configure) {
        PopupController popup = InitPopup("popupHonour");
        popup.payload = configure;
        return popup.Id;
    }

    public static int CertificateNotify(UserCertificate certificateMapping) {
        return EnqueuePopup(id => {
            PopupController popup = InitPopup("popupCertificateNotify", id);
            popup.payload = certificateMapping;
            return popup;
        });
       
    }

    public static int TrophyNotify(UserTrophy data) {
        PopupController popup = InitPopup("popupTrophyNotify");
        popup.payload = data;
        return popup.Id;
    }

    public static int HonorRank() {
        PopupController popup = InitPopup("popupHonourRank");
        return popup.Id;
    }
    public static int SetMatchHonor(string title, Action<Course_Race_Type> action) {
        PopupController popup = InitPopup("popupSetMatchHonor");
        popup.title = title;
        popup.payload = action;
        return popup.Id;
    }

    public static int SetTrophy(string title, PopupSetTrophyData data, Action close) {
        PopupController popup = InitPopup("popupSetTrophy");
        popup.title = title;
        popup.payload = data;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int SelectTrophyType(string title, Action<TrophySetting> action) {
        PopupController popup = InitPopup("popupSelectTrophyType");
        popup.title = title;
        popup.payload = action;
        return popup.Id;
    }

    public static int SelectTrophyBody() {
        PopupController popup = InitPopup("popupSelectTrophyBody");
        return popup.Id;
    }

    public static int ProjectView(Action<IRepositoryPath> onSelect,
                                  Action onClose = null, 
                                  bool showDeleteBtn = true, 
                                  bool showAddCell = true,
                                  IRepositoryPath initialDir = null)
    {
        var controller = InitPopup("popupProjectView");
        controller.payload = new ProjectViewPayload {
            showDeleteBtn = showDeleteBtn,
            selectCallback = onSelect,
            showAddCell = showAddCell,
            initialDir = initialDir
        };
        controller.closeAction = onClose;
        return controller.Id;
    }

    public static int VideoPlayer(string url)
    {
        var controller = InitPopup("popupVideoPlayer");
        controller.payload = url;
        return controller.Id;
    }

    public static int VideoPreview(SharedVideo video, Action<SharedVideo> onDeleted = null)
    {
        var controller = InitPopup("popupVideoPreview");
        controller.payload = new PopupVideoPreviewPayload {
            onVideoDeleted = onDeleted,
            video = video
        };
        return controller.Id;
    }

    public static int VideoShare()
    {
        var controller = InitPopup("popupVideoShare");
        return controller.Id;
    }

    public static int ShareVideoPlatform(SharedVideo video)
    {
        var controller = InitPopup("popupShareVideoPlatform");
        controller.payload = video;
        return controller.Id;
    }

    public static int ImagePreview(byte[] imageData , Action onClose = null) {
        var controller = (PopupImagePreview)InitPopup("PopupImagePreview");
        controller.SetImageData(imageData);
        controller.closeAction = onClose;
        return controller.Id;
    }

    public static int ImagePreview(string name, Action onClose = null)
    {
        var controller = (PopupImagePreview)InitPopup("PopupImagePreview");
        controller.SetImageName(name);
        controller.closeAction = onClose;
        return controller.Id;
    }

    public static int SetPassword(string title, string content, SetPasswordData setPasswordData)
    {
        var controller = InitPopup("popupPassword", ++s_lastPopupId);
        controller.payload = setPasswordData;
        controller.title = title;
        controller.content = content;
        return controller.Id;
    }

    public static PopupUploadGameboard UploadGameboard(string title, 
                                                       PopupUploadGameboardHandler uploadHandler,
                                                       Func<Gameboard.Gameboard, bool> filter = null)
    {
        if (uploadHandler == null)
        {
            throw new ArgumentNullException("uploadHandler");
        }

        var controller = InitPopup("popupUploadGameboard");
        controller.title = title;
        controller.payload = new PopupUploadGameboardPayload(uploadHandler, filter);
        return (PopupUploadGameboard)controller;
    }

    public static PopupSelectAttachment SelectAttachment(string title, 
                                                         IEnumerable<LocalResData> resources, 
                                                         IPopupSelectAttachmentDelegate eventDelegate,
                                                         Color? color = null,
                                                         bool editable = true)
    {
        if (eventDelegate == null)
        {
            throw new ArgumentNullException("eventDelegate");
        }

        var controller = InitPopup("popupSelectAttachment");
        controller.payload = new PopupSelectAttachment.Payload {
            resources = resources,
            eventDelegate = eventDelegate,
            themeColor = color,
            editable = editable,
        };
        controller.title = title;

        return (PopupSelectAttachment)controller;
    }

    public static void AttachmentManager(
        List<AttachData> attachdatas,
        Action<Action> onBeforeUploading,
        Action close,
        int programCount = PopupAttachmentManager.MaxAttachCount,
        int gameboardCount = 0,
        bool proGbMutex = false,
        bool showResouce = true,
        bool hideDelReal = false,
        int maxAttachCount = PopupAttachmentManager.MaxAttachCount)
    {
        var controller = InitPopup("popupAttachmentManager");
        controller.payload = new PopupAttachmentManager.Payload { attachDatas = attachdatas,
            programCount = programCount,
            gameboardCount = gameboardCount,
            onBeforeUploading = onBeforeUploading,
            proGbMutex = proGbMutex,
            showResource = showResouce,
            hideDelReal = hideDelReal,
            maxAttachCount = maxAttachCount
        };
        controller.closeAction = close;
    }

    public static void ViewStuSubAtch(List<AddAttachmentCellData> attachDatas) {
        var controller = InitPopup("PopupViewStuSubAtch");
        controller.payload = attachDatas;
    }

    public static PopupSinglePk SinglePk(Action onClose = null)
    {
        var controller = InitPopup("popupSinglePk");
        controller.closeAction = onClose;
        return (PopupSinglePk)controller;
    }

    public static PopupSinglePkDetail SinglePkDetail(GameBoard gameboard, bool showLeaderboardButton = true)
    {
        if (gameboard == null)
        {
            throw new ArgumentNullException();
        }

        var controller = (PopupSinglePkDetail)InitPopup("popupSinglePkDetail");
        controller.payload = gameboard;
        controller.ShowLeaderboardButton(showLeaderboardButton);
        return controller;
    }

    public static PopupAdminCompetition AdminCompetition(Action onClose = null)
    {
        var controller = InitPopup("popupAdminCompetition");
        controller.closeAction = onClose;
        return (PopupAdminCompetition)controller;
    }

    public static PopupEditCompetition EditCompetition(Competition competition, 
                                                       ICompetitionService service)
    {
        var controller = (PopupEditCompetition)InitPopup("popupEditCompetition");
        controller.Initialize(competition, service);
        return controller;
    }

    public static PopupCompetitionProblems CompetitionProblems(Competition competition,
                                                               ICompetitionService service,
                                                               bool allowEditing,
                                                               Action onClosed = null)
    {
        var controller = (PopupCompetitionProblems)InitPopup("popupCompetitionProblems");
        controller.closeAction = onClosed;
        controller.Initialize(competition, service, allowEditing);
        return controller;
    }

    public static PopupEditCompetitionProblem EditCompetitionProblem(Competition competition,
                                                                     CompetitionProblem problem,
                                                                     ICompetitionService service)
    {
        var controller = (PopupEditCompetitionProblem)InitPopup("popupEditCompetitionProblem");
        controller.Initialize(competition, problem, service);
        return controller;
    }

    public static PopupStudentCompetition StudentCompetition(Action onClose)
    {
        var controller = (PopupStudentCompetition)InitPopup("popupStudentCompetition");
        controller.closeAction = onClose;
        return controller;
    }

    public static PopupStudentCompetitionProblems StudentCompetitionProblems(Competition competition, 
                                                                             ICompetitionService service)
    {
        var controller = (PopupStudentCompetitionProblems)InitPopup("popupStudentCompetitionProblems");
        controller.Initialize(competition, service);
        return controller;
    }

    public static PopupCompetitionProblemDetail CompetitionProblemDetail(CompetitionProblem problem, 
                                                                         ICompetitionService service,
                                                                         bool showUploadButton = true)
    {
        var controller = (PopupCompetitionProblemDetail)InitPopup("popupCompetitionProblemDetail");
        controller.Initialize(problem, service, showUploadButton);
        return controller;
    }

    public static PopupCompetitionOverallLeaderboard CompetitionOverrallLeaderboard(Competition competition,
                                                                                    ICompetitionService service)
    {
        var controller = (PopupCompetitionOverallLeaderboard)InitPopup("popupCompetitionOverallLeaderboard");
        controller.Initialize(competition, service);
        return controller;
    }

    public static PopupController Settings()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UISystemSettingsDialog>();
        dialog.Configure(new PreferenceSettingsViewModel());
        dialog.OpenDialog();
        return dialog;
    }

    public static PopupPythonProjectView PythonProjectView(string initialPath = "", 
                                                      Action<IRepositoryPath> onSelected = null,
                                                      bool programFolderOnly = false,
                                                      Action onClosed = null)
    {
        var controller = InitPopup("popupPythonProjectView");
        controller.payload = new PythonProjectViewConfig {
            initialPath = initialPath,
            onSelected = onSelected,
            programFolderOnly = programFolderOnly,
        };
        controller.closeAction = onClosed;
        return (PopupPythonProjectView)controller;
    }

    public static PopupCompetitionProblemLeaderboard CompetitionProblemLeaderboard(
        CompetitionProblem problem, 
        ICompetitionService service,
        bool showMyRank)
    {
        var controller = (PopupCompetitionProblemLeaderboard)InitPopup("popupCompetitionProblemLeaderboard");
        controller.Initialize(problem, service, showMyRank);
        return controller;
    }

    public static int DuplicateInstance()
    {
        return Notice("ui_duplicate_instance_found".Localize(), Application.Quit);
    }

    public static int Purchase(string buyHint, int price, string buyButtonText, Action accept, Action reject = null)
    {
        var controller = (PopupPurchase)InitPopup("popupPurchase");
        controller.Initialize(buyHint, price, buyButtonText);
        controller.rightAction = accept;
        controller.leftAction = reject;
        return controller.Id;
    }

    public static int IntelligentLesson(Action onClose) {
        PopupController popup = InitPopup("PopupIntelligentLesson");
        popup.closeAction = onClose;
        return popup.Id;
    }

    public static int ILEditor(CourseInfo courseInfo, Action<CourseInfo> callBack) {
        PopupController popup = InitPopup("PopupILEditor");
        Course_Info proCourseInfo = null;
        if(courseInfo != null) {
            proCourseInfo = courseInfo.proCourseInfo;
        }
        popup.payload = new PopupILEditor.PayLoad {
            courseInfo = proCourseInfo,
            callBack = callBack
        };
        return popup.Id;
    }

    public static int ILPeriod(CourseInfo courseInfo) {
        PopupController popup = InitPopup("PopupILPeriod");
        popup.payload = courseInfo;
        return popup.Id;
    }

    public static int PublishPeriods(Course_Info courseInfo) {
        PopupController popup = InitPopup("PopupPublishPeriods");
        popup.payload = courseInfo;
        return popup.Id;
    }

    public static int PublishPeriodsItem(PublishPeriodItem.PayLoad payload) {
        PopupController popup = InitPopup("PopupPublishPeriodsItem");
        popup.payload = payload;
        return popup.Id;
    }
    
    public static int ILPeriodStu(string courseName, List<St_Period_Info> periodInfos, Action close) {
        PopupController popup = InitPopup("PopupILPeriodStu");
        popup.payload = new StudentPeriodUI.PayLoad {
            courseName = courseName,
            periodInfos = periodInfos
        };
        popup.closeAction = close;
        return popup.Id;
    }

    public static int ILPeriodItem(St_Period_Info stPeriodInfo, Action close) {
        PopupController popup = InitPopup("PopupILPeriodItem");
        popup.payload = stPeriodInfo;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int PopupAddCourse(OnlineCourseStudentController.ShowType showType, Action<Course_Info> addCallback) {
        PopupController popup = InitPopup("PopupAddCourse");
        popup.payload = new StudentAddCourse.Payload {
            showType = showType,
            addCallback = addCallback
        };
        return popup.Id;
    }

    public static int PopupAddMatchCourse(CompetitionListModel currentModel, Action close)
    {
        PopupController popup = InitPopup("PopupAddMatchCourse");
        popup.payload = currentModel;
        popup.closeAction = close;
        return popup.Id;
    }
    public static int MyClass(Action onClose = null) {
        PopupController popup = InitPopup("PopupMyClass");
        popup.closeAction = onClose;
        return popup.Id;
    }
    public static int EditorClass(uint classId, ScriptLanguage classType, UITeacherEditClass.WorkMode workMode, Action refreshBack) {
        PopupController popup = InitPopup("PopupEditorClass");
        popup.payload = new UITeacherEditClass.PayLoad {
            classId = classId,
            classType = classType,
            workMode = workMode,
            refreshBack = refreshBack
        };
        return popup.Id;
    }

    public static int ClassInfo(Action close) {
        PopupController popup = InitPopup("PopupClassInfo");
        popup.closeAction = close;
        return popup.Id;
    }

    public static int AddClass() {
        PopupController popup = InitPopup("PopupAddClass");
        return popup.Id;
    }

    public static PopupWorkspace Workspace(CodeSceneArgs args, Action onClosed = null)
    {
        var popup = (PopupWorkspace)InitPopup("popupWorkspace");
        popup.payload = args;
        popup.closeAction = onClosed;
        return popup;
    }

    public static int EditorTask(UITeacherEditTask.WorkMode mode, Action refresh = null, uint TaskId = 0,
        TaskTemplate task = null, UITeacherTaskPool teacherTaskPool = null, PoolTaskEditCallback poolTaskUpdated = null) {
        PopupController popup = InitPopup("PopupEditorTask");
        popup.payload = new UITeacherEditTask.Payload {
            workMode = mode,
            refreshCallBack = refresh,
            editorTaskId = TaskId,
            task = task,
            teacherTaskPool = teacherTaskPool,
            poolTaskUpdated = poolTaskUpdated
        };
        return popup.Id;
    }

    public static int StudentTasks(IKickNotificationEvent eventSource) {
        if (eventSource == null)
        {
            throw new ArgumentNullException("eventSource");
        }

        PopupController popup = InitPopup("PopupStudentTasks");
        popup.payload = eventSource;
        return popup.Id;
    }

    public static int SelectPoolTask(Action<SelectPoolTaskInfo> action) {
        PopupController popup = InitPopup("PopupSelectPoolTask");
        popup.payload = action;
        return popup.Id;
    }

    public static int Exercises(Action close) {
        PopupController popup = InitPopup("PopupExercises");
        popup.closeAction = close;
     //   popup.payload = action;
        return popup.Id;
    }

    public static int GameBoardBank(Action<PopupGameBoardBank.SelectData> selectBack, Action close) {
        PopupController popup = InitPopup("PopupGameBoardBank");
        popup.payload = selectBack;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int EditorExercises(PopupEditorExercises.PayLoad data, Action close = null) {
        PopupController popup = InitPopup("PopupEditorExercises");
        popup.payload = data;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int AddExercise(PopupAddExercise.Payload data, Action close = null) {
        PopupController popup = InitPopup("PopupAddExercise");
        popup.payload = data;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int ExerciseDetail(PopupExerciseDetail.Payload data, Action close = null) {
        PopupController popup = InitPopup("popupExerciseDetail");
        popup.payload = data;
        popup.closeAction = close;
        return popup.Id;
    }

    public static int Exception(string stackTrace)
    {
        var popup = InitPopup("popupException");
        popup.content = stackTrace;
        return popup.Id;
    }

    public static int PeirodRank(PopupPeriodRank.PayLoad payload)
    {
        var popup = InitPopup("PopupPeriodRank");
        popup.payload = payload;
        return popup.Id;
    }

    private static PopupController InitPopup(string prefabName, int popupId = -1) {
        if (popupId == -1)
        {
            popupId = ++s_lastPopupId;
        }

        GameObject go = GameObject.Instantiate(Resources.Load(GetPrefabPath(prefabName),
                                                              typeof(GameObject)),
                                                              new Vector3(0, 0, 0),
                                                              Quaternion.identity) as GameObject;
        var controller = go.GetComponent<PopupController>();
        int sortingOrder = s_kMinSortingOrder;
        if (popupStack.Count > 0)
        {
            var lastController = popupStack.Last();
            sortingOrder = lastController.BaseSortingOrder + lastController.SortingLayers;
        }

        controller.Id = popupId;
        controller.BaseSortingOrder = sortingOrder;
        popupStack.Add(controller);
        if (controller.fullscreenOpaque)
        {
            HideAllStackedPopups();
        }
        UpdateAppCloseButton();

        return controller;
    }

    private static void HideAllStackedPopups()
    {
        for (int i = 0; i < popupStack.Count - 1; ++i)
        {
            popupStack[i].Show(false);
        }
    }

    private static string GetPrefabPath(string prefabName) {
        return "popup/" + prefabName ;
    }

    internal static void onClosing(PopupController controller)
    {
        if (controller == activeQueuedPopup)
        {
            activeQueuedPopup = null;
        }

        int index = popupStack.IndexOf(controller);
        if (index != -1)
        {
            var lastPopup = index == popupStack.Count - 1;
            popupStack.RemoveAt(index);
            // if the removed popup is the last or the first visible full screen pop up,
            // we need to show all pop ups till the next full screen pop up
            if (index > 0 && (lastPopup || controller.fullscreenOpaque && controller.isVisible))
            {
                PopupController current;
                do
                {
                    current = popupStack[--index];
                    current.Show(true);
                }
                while (!current.fullscreenOpaque && index > 0);
            }

            UpdateAppCloseButton();
        }
    }

    private static void UpdateAppCloseButton()
    {
        if (popupStack.Count > 0)
        {
            WindowUtils.EnableCloseButton(!popupStack.Last().isModal);
        }
        else
        {
            WindowUtils.EnableCloseButton(true);
        }
    }

    private void Update()
    {
        if (activeQueuedPopup == null && queuedPopups.Count > 0)
        {
            var popup = queuedPopups.First.Value;
            queuedPopups.RemoveFirst();
            activeQueuedPopup = popup.factory(popup.popupId);
        }
    }

    /// <summary>
    /// close the given popup
    /// </summary>
    /// <param name="id"></param>
    public static void Close(int id)
    {
        var index = popupStack.FindIndex(x => x && x.Id == id);
        if (index != -1)
        {
            if (popupStack[index])
            {
                popupStack[index].Close();
            }
        }
        else
        {
            queuedPopups.Remove(new QueuedPopup { popupId = id });
        }
    }

    /// <summary>
    /// close the given popup and clear the id
    /// </summary>
    /// <param name="id"></param>
    public static void Close(ref int id)
    {
        Close(id);
        id = 0;
    }

    public static void Close(PopupController controller)
    {
        // in case the popup was destroyed before the client has a chance to close it
        if (!ReferenceEquals(controller, null))
        {
            Close(controller.Id);
        }
    }

    public static void CloseAll()
    {
        while (popupStack.Count > 0)
        {
            var popupController = popupStack[popupStack.Count - 1];
            popupStack.RemoveAt(popupStack.Count - 1);
            // the popup has been destroyed somewhere
            if (popupController)
            {
                popupController.Close();
            }
        }

        ClearQueuedPopups();
    }

    /// <summary>
    /// clear all queue popup requests
    /// </summary>
    public static void ClearQueuedPopups()
    {
        queuedPopups.Clear();
    }

    internal static PopupController GetPrevious(PopupController current)
    {
        var index = popupStack.FindIndex(x => x == current);
        return index > 0 ? popupStack[index - 1] : null;
    }

    /// <summary>
    /// find the first popup with the given type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Find<T>() where T : PopupController
    {
        return (T)popupStack.LastOrDefault(x => x is T);
    }

    /// <summary>
    /// create the popup from the given resource name
    /// </summary>
    public static PopupController Create(string name)
    {
        return InitPopup(name);
    }

    internal static void BringToTop(PopupController controller)
    {
        if (popupStack.Count > 1 && popupStack.Last() != controller && popupStack.Remove(controller))
        {
            var oldTopMostController = popupStack.Last();
            popupStack.Add(controller);
            controller.BaseSortingOrder = oldTopMostController.BaseSortingOrder + oldTopMostController.SortingLayers;
        }
    }
}
