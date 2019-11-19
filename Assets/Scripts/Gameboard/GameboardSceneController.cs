using System;
using System.Collections;

namespace Gameboard
{
    public class GameboardScenePayload
    {
        public readonly int templateId;
        public readonly IRepositoryPath path;

        public GameboardScenePayload(int templateId, IRepositoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            this.templateId = templateId;
            this.path = path;
        }
    }

    public class GameboardSceneController : SceneController
    {
        public UIGameboard uiGameboard;

        public override void Init(object userData, bool isRestored)
        {
            base.Init(userData, isRestored);
            StartCoroutine(InitImpl(userData));
        }

        IEnumerator InitImpl(object userData)
        {
#if !DISABLE_PYTHON_SCRIPTING
            uiGameboard.SetLanguage(Preference.scriptLanguage);
#endif

            var payload = userData as GameboardScenePayload;
            if (payload != null)
            {
                if (payload.templateId != 0)
                {
                    yield return uiGameboard.New(payload.templateId);
                }
                else
                {
                    yield return uiGameboard.Open(payload.path.ToString());
                }
            }
            else
            {
                yield return uiGameboard.Open();
            }
            uiGameboard.SetWorkingDirectory(payload.path.parent.ToString());
            uiGameboard.onClosing.AddListener(OnClosedGameboard);
        }

        private void OnClosedGameboard()
        {
            SceneDirector.Pop();
        }

        public override bool OnKey(KeyEventArgs eventArgs)
        {
            return false;
            // temporarily disable back key logic until the gameboard handles back key correctly
            /*
            if (uiGameboard.isRecording)
            {
                return false;
            }

            return base.OnKey(eventArgs);*/
        }
    }
}
