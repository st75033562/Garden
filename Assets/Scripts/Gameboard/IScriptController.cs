using System;
using System.Collections;
using System.Collections.Generic;

namespace Gameboard
{
    public interface IScriptController
    {
        void Uninitialize();

        IEnumerator PrepareRunning();

        void Run();

        void SetPaused(bool paused);

        void Stop();

        void SetGameboard(Gameboard gameboard);

        // userGroups must not be null
        // defaultGroups may be null
        void InitCodeBindings(RobotCodeGroups userGroups, RobotCodeGroups defaultGroups, Action done, Action onError, List<string> path = null);

        void EditCode(int robotIndex);

        void AssignCode(int robotIndex, bool canCreateNew, Action onCodeAssigned = null);

        void UnassignCode(int robotIndex);

        bool IsCodeAssigned(int robotIndex);

        bool IsUserCodeAssigned(int robotIndex);

        string GetRobotCodePath(int robotIndex);
    }
}
