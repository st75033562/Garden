using Gameboard.Script;
using Google.Protobuf;
using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gameboard
{
    // #TODO handle project removal
    // #TODO extract runtime control methods from IScriptController into IScriptRuntimeController
    public class PythonScriptController : MonoBehaviour, IScriptController
    {
        public const string absPath = "__absPath.abs";
        // for now, only one script can be used
        private string m_scriptPath = "";
        private bool m_scriptPathAbs;
        private Gameboard m_gameboard;
        private RobotCodeGroups m_codeGroups;

        private IScriptHandle m_runningScript;
        private TcpServer m_server;
        private UIGameboard m_uiGameboard;
        private bool m_isRunning;

        private readonly RobotSensorNotifier m_robotSensorNotifier = new RobotSensorNotifier();
        private readonly SetEffectorHandler m_setEffectorHandler = new SetEffectorHandler();
        private VariableTracker m_variableUpdator;

        public void Initialize(UIGameboard uiGameboard)
        {
            m_uiGameboard = uiGameboard;
            m_variableUpdator = new VariableTracker(uiGameboard.codingSpace.CodeContext.variableManager);
        }

        public void Uninitialize()
        {
            if (m_server != null)
            {
                m_server.Stop();
            }

            Stop();
            m_uiGameboard = null;
        }

        public IEnumerator PrepareRunning()
        {
            m_robotSensorNotifier.Reset();
            m_robotSensorNotifier.robotManager = m_uiGameboard.robotManager;
            m_setEffectorHandler.robotManager = m_uiGameboard.robotManager;

            if (m_server == null)
            {
                m_server = gameObject.AddComponent<TcpServer>();
                m_server.RequestParser = ScriptRequestParser.instance;
                m_server.OnConnectionEstablished += OnConnectionEstablished;
                m_server.OnConnectionClosed += OnConnectionClosed;
                RegisterHandlers();
                m_server.Run();

                Debug.Log("gameboard server listening at: " + m_server.Port);
            }

            yield break;
        }

        private void RegisterHandlers()
        {
            m_server.RegisterHandler(CommandId.CmdSetEffector, m_setEffectorHandler.Handle);
            m_server.RegisterHandler(CommandId.CmdJoin, OnClientJoin);
            m_server.RegisterHandler(CommandId.CmdUpdateVariable, OnUpdateVariable);
        }

        #region netowrk handlers

        private void OnConnectionEstablished(ClientConnection conn)
        {
            // for now we assume only one client can connect the server
            m_variableUpdator.connection = conn;
            m_robotSensorNotifier.connection = conn;
        }

        private void OnConnectionClosed(ClientConnection conn)
        {
            m_robotSensorNotifier.connection = null;
            m_variableUpdator.connection = null;

            // reset all robots
            foreach (var robot in m_uiGameboard.robotManager)
            {
                robot.resetDevices();
            }
        }

        private void OnClientJoin(ClientConnection conn, IMessage msg)
        {
            var request = (JoinRequest)msg;
            m_robotSensorNotifier.SetMaxRobotNum(request.RobotNum);

            var response = new JoinResponse();
            response.RobotNum = Mathf.Min(m_uiGameboard.robotManager.robotCount, request.RobotNum);

            foreach (var variable in m_uiGameboard.codingSpace.CodeContext.variableManager)
            {
                if (variable.scope == NameScope.Global)
                {
                    var variableInfo = new Variable();
                    variableInfo.Name = RobotCodeManager.GlobalDataPrefix + variable.name;
                    variableInfo.Type = (VariableType)variable.type;
                    variableInfo.Value = VariableJsonUtils.ValueToJson(variable);
                    variableInfo.Writable = variable.globalVarOwner != GlobalVarOwner.Gameboard;

                    response.Variables.Add(variableInfo);
                }
            }
            conn.Send(CommandId.CmdJoin, response);
        }

        private void OnUpdateVariable(ClientConnection conn, IMessage msg)
        {
            m_variableUpdator.OnChange((UpdateVariableRequest)msg);
            conn.Send(CommandId.CmdUpdateVariable, new UpdateVariableResponse());
        }

        #endregion

        public void Run()
        {
            if (m_runningScript != null)
            {
                throw new InvalidOperationException();
            }
            if (m_scriptPath != "")
            {
                var environ = new Dictionary<string, string> {
                    { "SERVER_PORT", m_server.Port.ToString() },
#if UNITY_EDITOR
                    { "GB_DEBUG", "1" },
#endif
                };
                string absPath = m_scriptPath;
                if(!m_scriptPathAbs) {
                    absPath = PythonRepository.instance.getAbsPath(m_scriptPath);
                }
                
                m_runningScript = ScriptUtils.Run(absPath, environ);
            }
            m_isRunning = true;
        }

        public void SetPaused(bool paused)
        {
            // TODO:
        }

        public void Stop()
        {
            m_isRunning = false;
            if (m_server != null)
            {
                m_server.CloseConnections();
            }
            if (m_runningScript != null)
            {
                // TODO: kill async
                m_runningScript.KillTree();
                m_runningScript.Dispose();
                m_runningScript = null;
            }
        }

        void Update()
        {
            if (m_isRunning)
            {
                m_robotSensorNotifier.Notify();
                m_variableUpdator.SendUpdates();
            }
        }

        public void SetGameboard(Gameboard gameboard)
        {
            if (m_gameboard != null)
            {
                m_gameboard.onRobotAdded -= OnRobotAdded;
                m_gameboard.onRobotRemoved -= OnRobotRemoved;
            }
            m_gameboard = gameboard;
            m_gameboard.onRobotAdded += OnRobotAdded;
            m_gameboard.onRobotRemoved += OnRobotRemoved;
        }

        public void InitCodeBindings(
            RobotCodeGroups userGroups, RobotCodeGroups defaultGroups,
            Action done, Action onError, List<string> path = null) {
            DownLoadRelation(path, ()=> {
                m_scriptPath = "";
                m_codeGroups = userGroups;

                if(defaultGroups != null) {
                    if(defaultGroups.codeGroups.ToList().Count <= 0) {
                        done();
                    } else {
                        foreach(var group in defaultGroups.codeGroups) {
                            DownLoad(group.projectPath, (scriptPath) => {
                                m_scriptPathAbs = true;
                                m_scriptPath = scriptPath;
                                done();
                            });
                        }
                    }
                } else {
                    foreach(var group in m_codeGroups.codeGroups) {
                        var absPath = PythonRepository.instance.getAbsPath(group.projectPath);
                        if(File.Exists(absPath)) {
                            m_scriptPathAbs = false;
                            m_scriptPath = group.projectPath;
                        } else {
                            Debug.LogFormat("script {0} not found, break connection", group.projectPath);
                        }
                        break;
                    }
                    done();
                }
            });
        }

        void DownLoadRelation(List<string> paths, Action done) {
            if(paths == null || paths.Count == 0) {
                done();
                return;
            }
            string path = paths[0];
            paths.Remove(path);
            var request = new ProjectDownloadRequest();
            request.basePath = ProjectUrl.GetPath(path);
            request.preview = true;
            request.blocking = true;
            request.Success(dir => {
                for(int i = 0; i < dir.FileList_.Count; ++i) {
                    FileNode tCurFile = dir.FileList_[i];
                    string fullPath = FileUtils.appTempPath + "/" + tCurFile.PathName;
                    if((FN_TYPE)tCurFile.FnType == FN_TYPE.FnDir) {
                        FileUtils.createParentDirectory(fullPath);
                    } else {
                        FileUtils.createParentDirectory(fullPath);
                        File.WriteAllBytes(fullPath, tCurFile.FileContents.ToByteArray() ?? new byte[0]);
                    }
                }
                DownLoadRelation(paths, done);
            })
                .Execute();
        }

        private void DownLoad(string path, Action<string> done) {
            var request = new ProjectDownloadRequest();
            request.basePath = ProjectUrl.GetPath(path);
            request.preview = true;
            request.blocking = true;
            request.Success(dir => {
                string mainScripath = null;
                for(int i = 0; i < dir.FileList_.Count; ++i) {
                    FileNode tCurFile = dir.FileList_[i];
                    string fullPath = FileUtils.appTempPath + "/" + tCurFile.PathName;
                    if((FN_TYPE)tCurFile.FnType == FN_TYPE.FnDir) {
                        FileUtils.createParentDirectory(fullPath);
                    } else {
                        FileUtils.createParentDirectory(fullPath);
                        mainScripath = fullPath;
                        File.WriteAllBytes(fullPath, tCurFile.FileContents.ToByteArray() ?? new byte[0]);
                    }
                }
                done(mainScripath);
            })
                .Execute();
        }

        private void OnRobotAdded(int robotIndex)
        {
            m_scriptPathAbs = false;
            m_codeGroups.SetRobotCode(robotIndex, m_scriptPath);
        }

        private void OnRobotRemoved(int robotIndex)
        {
            m_codeGroups.SetRobotCode(robotIndex, null);
        }

        public void EditCode(int robotIndex)
        {
            if (m_scriptPath == "")
            {
                throw new InvalidOperationException();
            }
            string scripathAbs = m_scriptPath;
            if(!m_scriptPathAbs) {
                scripathAbs = PythonRepository.instance.getAbsPath(m_scriptPath);
            }
            PythonEditorManager.instance.Open(scripathAbs);
        }

        public void AssignCode(int robotIndex, bool canCreateNew, Action onCodeAssigned)
        {
            string path = "";
            if (m_scriptPath != "" && !m_scriptPathAbs)
            {
                path = Path.GetDirectoryName(m_scriptPath);
            }
            PopupManager.PythonProjectView(path, projectPath => {
                m_scriptPath = projectPath.ToString();
                m_scriptPathAbs = false;
                // assign code to all robots
                for (int i = 0; i < m_gameboard.robots.Count; ++i)
                {
                    m_codeGroups.SetRobotCode(i, m_scriptPath);
                }

                m_uiGameboard.undoManager.SetClean(false);

                if (onCodeAssigned != null)
                {
                    onCodeAssigned();
                }
            });
        }

        public void UnassignCode(int robotIndex)
        {
            m_uiGameboard.undoManager.SetClean(false);
            m_scriptPath = "";
            m_scriptPathAbs = false;
            m_codeGroups.ClearCodeGroups();
        }

        public bool IsCodeAssigned(int robotIndex)
        {
            return m_scriptPath != "";
        }

        public bool IsUserCodeAssigned(int robotIndex)
        {
            return IsCodeAssigned(robotIndex);
        }

        public string GetRobotCodePath(int robotIndex)
        {
            if(m_scriptPathAbs) {
                return absPath;
            } else {
                return m_scriptPath;
            }
        }
    }
}
