using System;
using UnityEngine.Events;

namespace Gameboard
{
    public interface IGameboardPlayer
    {
        UnityEvent onStartRunning { get; }

        UnityEvent onStopRunning { get; }

        BoolUnityEvent onPauseRunning { get; }

        void Run();

        void RunAndSubmit();

        void Stop();

        void Pause(bool pause);

        void Restart();

        bool isRunning { get; }

        bool isPaused { get; }
    }
}
