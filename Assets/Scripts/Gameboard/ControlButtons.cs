using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public class ControlButtons : MonoBehaviour
    {
        public Button m_runButton;
        public Button m_stopButton;
        public UIPauseButton m_pauseButton;

        private IGameboardPlayer m_player;

        void Start()
        {
            m_runButton.onClick.AddListener(OnClickStart);
            m_stopButton.onClick.AddListener(OnClickStop);
            m_pauseButton.onClick.AddListener(OnClickPause);
        }

        public void Init(IGameboardPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException("player");
            }

            player.onStartRunning.AddListener(Refresh);
            player.onStopRunning.AddListener(Refresh);
            player.onPauseRunning.AddListener(delegate { Refresh(); });

            m_player = player;

            Refresh();
        }

        void OnClickStart()
        {
            m_player.Run();
        }

        void OnClickStop()
        {
            m_player.Stop();
        }

        void OnClickPause()
        {
            m_player.Pause(!m_player.isPaused);
        }

        void Refresh()
        {
            m_runButton.gameObject.SetActive(!m_player.isRunning);
            m_stopButton.gameObject.SetActive(m_player.isRunning);

            m_pauseButton.isPaused = m_player.isPaused;
            m_pauseButton.interactable = m_player.isRunning;
        }
    }
}
