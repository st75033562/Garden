//#define DEBUG_GAMEBOARD

using System;

using g_WebRequestManager = Singleton<WebRequestManager>;

namespace Gameboard
{
    class GameboardSaveController : SaveCodeControllerBase
    {
        private readonly UIGameboard m_uiGameboard;
        private readonly Func<bool> m_changed;

        public GameboardSaveController(UIGameboard gameboard, Func<bool> changed)
        {
            m_uiGameboard = gameboard;
            m_changed = changed;
        }

        public override bool isChanged
        {
            get { return m_changed(); }
        }

        protected override string currentProjectName
        {
            get { return m_uiGameboard.GetGameboard().name; }
        }

        protected override IDialogInputValidator CreateProjectNameValidator()
        {
            return new ProjectNameValidator(m_uiGameboard.codingSpace.WorkingDirectory,
                currentProjectName, GameboardRepository.instance);
        }

        protected override void SaveAndSynchro(string name, Action onSaved, Action onSaveError)
        {
            var project = m_uiGameboard.GetGameboardProject();
            var projectPath = m_uiGameboard.codingSpace.WorkingDirectory + name;

            var request = Uploads.UploadGameboardV3(project, projectPath);
            request.Success(() => {
                m_uiGameboard.codingSpace.ProjectName = name;
                m_uiGameboard.GetGameboard().name = name;

                GameboardRepository.instance.save("", request.files.FileList_);

                m_uiGameboard.OnGameboardSaved();

                if (onSaved != null)
                {
                    onSaved();
                }
            })
            .Error(onSaveError)
            .Execute();
        }
    }
}
