using System;
using Google.Protobuf;

public class SinglePkChallengeHelper
{
    public event Action onUploaded;

    private readonly GameBoard m_gameboard;

    public SinglePkChallengeHelper(GameBoard gameboard)
    {
        //UnityEngine.SceneManagement.SceneManager.UnloadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName("Lobby"));
        if (gameboard == null)
        {
            throw new ArgumentNullException("gameboard");
        }
        m_gameboard = gameboard;
    }

    public void UploadAnswer()
    {
        if (!m_gameboard.CanUserUploadAnswer(UserManager.Instance.UserId))
        {
            PopupManager.Notice("ui_single_pk_prohibit_repeated_answer".Localize());
            return;
        }
        PopupManager.ProjectView(OnSelectedProject, showDeleteBtn:false, showAddCell:false);
    }

    void OnSelectedProject(IRepositoryPath path)
    {
        PopupManager.TwoBtnDialog(
            "gameboard_upload_notice".Localize(),
            "gameboard_upload_yes".Localize(),
            () => {
                EvaluateAnswer(path, true, PopupILPeriod.PassModeType.Submit);
            },
            "gameboard_upload_no".Localize(),
            () => {
                EvaluateAnswer(path, false, PopupILPeriod.PassModeType.Submit);
            });
    }

    public void EvaluateAnswer(IRepositoryPath path, bool openSource, PopupILPeriod.PassModeType passMode)
    {
        int popupId = 0;
        RobotCodeInfo[] robotCodes = null;
        if(path != null) {
            robotCodes = new[] { RobotCodeInfo.Local(path.ToString(), UserManager.Instance.Nickname) } ;
        }
        popupId = PopupManager.GameboardPlayer(
            ProjectPath.Remote(m_gameboard.ProjPath),
            robotCodes,
            (mode, result) => {
                if(passMode == mode) {
                    PopupManager.Close(popupId);
                    var newScore = 0;
                    if(passMode == PopupILPeriod.PassModeType.Submit) {
                        newScore = result.robotScores[0];
                    } else {
                        newScore = result.sceneScore;
                    }
                    var oldAnswer = m_gameboard.GetUserAnswer(UserManager.Instance.UserId);
                    if(oldAnswer != null) {
                        PopupManager.YesNo("ui_pk_upload_overwrite_confirm".Localize(oldAnswer.GbScore, newScore),
                                           () => UploadAnswer(path, newScore, openSource));
                    } else {
                        UploadAnswer(path, newScore, openSource);
                    }
                }
            }, noTopBarMode:true);
    }

    void UploadAnswer(IRepositoryPath path, int score, bool openSource)
    {
        GBAnswer answer = new GBAnswer();
        answer.GbScriptShow = (uint)(openSource ? GbScriptShowType.Show : GbScriptShowType.Hide);
        if(path != null) {
            answer.AnswerName = UserManager.Instance.Nickname + "_" + path.name;
        } else {
            answer.AnswerName = UserManager.Instance.Nickname;
        }
        
        answer.GbScore = score;

        var request = new CMD_Answer_Gameboard_r_Paramerers();
        request.GbId = m_gameboard.GbId;
        request.GbAnswerInfo = answer;

        if(path != null) {
            var project = CodeProjectRepository.instance.loadCodeProject(path.ToString());
            if(project == null) {
                PopupManager.Notice("ui_failed_to_load_project".Localize());
                return;
            }

            request.GbFiles = new FileList();
            request.GbFiles.FileList_.AddRange(project.ToFileNodeList(""));
        }

        int popupId = PopupManager.ShowMask();

        SocketManager.instance.send(Command_ID.CmdAnswerGameboardR, request.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if (res == Command_Result.CmdNoError)
            {
                var createGameboardA = CMD_Answer_Gameboard_a_Paramerers.Parser.ParseFrom(content);
                m_gameboard.SetAnswer(createGameboardA.GbAnswerInfo.AnswerId, createGameboardA.GbAnswerInfo);

                if (onUploaded != null)
                {
                    onUploaded();
                }
                PopupManager.Notice("ui_submit_sucess".Localize());
            }
            else
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }
}
