using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    class GameboardCodingController : MonoBehaviour
    {
        public UIGameboard uiGameboard;

        private GameboardSaveController m_saveController;

        void Start()
        {
            m_saveController = new GameboardSaveController(uiGameboard, () => {
                return uiGameboard.codingSpace.IsChanged;
            });

            GetComponent<ControlButtons>().Init(uiGameboard);

            var workspace = GetComponent<UIWorkspace>();
            workspace.OnSceneViewClicked += delegate { OnClickReturn(); };
            workspace.OnSystemMenuClicked += OnClickSystemMenu;
            workspace.OnBackClicked += OnClickReturn;
        }

        void OnClickSystemMenu()
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UIOperationDialog>();
            var config = new UIOperationDialogConfig();
            config.AddButton(UIOperationButton.New, OnClickNew);
            config.AddButton(UIOperationButton.Save, OnClickSave);
            config.AddButton(UIOperationButton.Open, OnClickOpen);
            dialog.Configure(config);
            dialog.OpenDialog();
        }

        void OnClickReturn()
        {
            uiGameboard.CloseCodingSpace();
        }

        public void OnClickNew()
        {
            SaveAndOpen();
        }

        private void SaveAndOpen()
        {
            m_saveController.SaveWithConfirm(status => {
                OpenAndClose();
            });
        }

        private void OpenAndClose()
        {
            uiGameboard.SelectAndOpen(() => {
                uiGameboard.CloseCodingSpace();
            });
        }

        public void OnClickSave()
        {
            m_saveController.SaveAs(() => { });
        }

        public void OnClickShare()
        {
            m_saveController.SaveAs(() => {
                int popupId = PopupManager.ShowMask();

                GameBoardSession gameboarSession = new GameBoardSession();
                gameboarSession.ShareGameBoard(uiGameboard.GetGameboardProject(), (res, content) => {
                    PopupManager.Close(popupId);
                    if (res == Command_Result.CmdNoError)
                    {
                        CMD_Create_Gameboard_a_Parameters createGameboardA = CMD_Create_Gameboard_a_Parameters.Parser.ParseFrom(content);
                        Debug.Log("====>" + createGameboardA.GbId);
                    }
                    else
                    {
                        PopupManager.Notice(res.Localize());
                    }
                });
            });
        }

        public void OnClickOpen()
        {
            SaveAndOpen();
        }
    }

}
