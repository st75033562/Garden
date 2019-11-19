using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using g_WebRequestManager = Singleton<WebRequestManager>;

namespace Gameboard
{
    public class VisualScriptController : IScriptController
    {
        private readonly UIGameboard m_uiGameboard;
        private readonly Dictionary<string, Project> m_defaultProjects = new Dictionary<string, Project>();
        private Gameboard m_gameboard;
        private ProjectDownloadRequestV3 m_currentTask;
        private RobotCodeGroups m_userGroups;
        private RobotCodeGroups m_defaultGroups;

        private const string DefaultProjectPrefix = "default:";

        public VisualScriptController(UIGameboard uiGameboard)
        {
            if (uiGameboard == null)
            {
                throw new ArgumentNullException("cuiGameboard");
            }

            m_uiGameboard = uiGameboard;
            CodeProjectRepository.instance.onProjectDeleted += OnProjectDeleted;
        }

        public void Uninitialize()
        {
            if (m_currentTask != null)
            {
                m_currentTask.Abort();
                m_currentTask = null;
            }
            CodeProjectRepository.instance.onProjectDeleted -= OnProjectDeleted;
        }

        public IEnumerator PrepareRunning()
        {
            yield return m_uiGameboard.codeManager.PrepareRunning();
        }

        public void Run()
        {
            m_uiGameboard.codeManager.Run();
        }

        public void SetPaused(bool paused)
        {
            m_uiGameboard.codeManager.SetPaused(paused);
        }

        public void Stop()
        {
            m_uiGameboard.codeManager.Stop();
        }

        public void SetGameboard(Gameboard gameboard)
        {
            if (m_gameboard != null)
            {
                m_gameboard.onRobotRemoved -= OnRobotRemoved;
            }

            m_gameboard = gameboard;
            m_gameboard.onRobotRemoved += OnRobotRemoved;
        }

        public void InitCodeBindings(
            RobotCodeGroups userGroups, RobotCodeGroups defaultGroups, 
            Action done, Action onError, List<string> path = null)
        {
            m_defaultProjects.Clear();

            m_userGroups = userGroups;
            m_defaultGroups = defaultGroups;

            var userProjects = new Dictionary<string, Project>();
            Download(userGroups, userProjects, () => {
                Download(defaultGroups, m_defaultProjects, () => {
                    InitializeCodeBindings(userProjects);
                    done();
                }, onError);
            }, onError);
        }

        private void Download(RobotCodeGroups groups, Dictionary<string, Project> projects, Action done, Action onError)
        {
            if (groups != null)
            {
                var groupInfo = groups.codeGroups.ToArray();
                Download(groupInfo, projects, 0, done, onError);
            }
            else
            {
                done();
            }
        }

        private void Download(
            RobotCodeGroupInfo[] groups, Dictionary<string, Project> projects, int index, Action done, Action onError)
        {
            if (index >= groups.Length)
            {
                m_currentTask = null;
                done();
                return;
            }

            var projectUrl = groups[index].projectPath;
            if (ProjectUrl.IsRemote(projectUrl))
            {
                m_currentTask = new ProjectDownloadRequestV3();
                m_currentTask.basePath = ProjectUrl.GetPath(projectUrl);
                m_currentTask.preview = true;
                m_currentTask.defaultErrorHandling = false;
                m_currentTask
                    .Success(dir => {
                        var project = dir.ToProject();
                        project.name = groups[index].projectName;
                        projects.Add(projectUrl, project);

                        Download(groups, projects, index + 1, done, onError);
                    })
                    .Error(() => {
                        Debug.LogError("failed to download " + projectUrl);
                        onError();
                    })
                    .Finally(() => m_currentTask = null)
                    .Execute();
            }
            else
            {
                var path = ProjectUrl.GetPath(projectUrl);
                var project = CodeProjectRepository.instance.loadCodeProject(path);
                if (project != null)
                {
                    projects.Add(projectUrl, project);
                }
                else
                {
                    Debug.LogError("failed to load " + path);
                }

                Download(groups, projects, index + 1, done, onError);
            }
        }

        // return the unique path to a default project
        // default projects can be remote if prefixed with : or local if prefixed with nothing
        private static string GetDefaultProjectPath(string path)
        {
            return DefaultProjectPrefix + path;
        }

        private static string GetOriginalProjectPath(string path)
        {
            if (path.StartsWith(DefaultProjectPrefix))
            {
                return path.Substring(DefaultProjectPrefix.Length);
            }
            return path;
        }

        private void InitializeCodeBindings(Dictionary<string, Project> userProjects)
        {
            m_uiGameboard.codeManager.onRobotGroupChanged -= OnRobotGroupChanged;

            // set unique project path for default projects
            foreach (var path in m_defaultProjects.Keys.ToArray())
            {
                var project = m_defaultProjects[path];
                m_defaultProjects.Remove(path);
                m_defaultProjects[GetDefaultProjectPath(path)] = project;
            }

            var invalidGroups = new List<RobotCodeGroupInfo>();
            for (int i = 0; i < m_gameboard.robots.Count; ++i)
            {
                var groupInfo = m_userGroups.GetGroup(i);
                if (groupInfo != null)
                {
                    if (userProjects.ContainsKey(groupInfo.projectPath))
                    {
                        var codeGroup = m_uiGameboard.codeManager.GetOrCreateGroup(groupInfo.projectPath);
                        if (codeGroup.project == null)
                        {
                            codeGroup.project = userProjects[groupInfo.projectPath];
                            if (!ProjectUrl.IsRemote(groupInfo.projectPath))
                            {
                                codeGroup.workingDirectory = Path.GetDirectoryName(groupInfo.projectPath);
                            }
                        }
                        m_uiGameboard.codeManager.AddRobotToGroup(i, codeGroup);
                    }
                    else if (!invalidGroups.Contains(groupInfo))
                    {
                        invalidGroups.Add(groupInfo);
                    }
                }
                else if (m_defaultGroups != null)
                {
                    SetRobotDefaultCode(i);
                }
            }

            foreach (var g in invalidGroups)
            {
                m_userGroups.RemoveCodeGroup(g);
            }

            m_uiGameboard.codeManager.onRobotGroupChanged += OnRobotGroupChanged;
        }

        private void SetRobotDefaultCode(int index)
        {
            if (m_defaultGroups == null) { return; }

            var groupInfo = m_defaultGroups.GetGroup(index);
            if (groupInfo == null)
            {
                return;
            }

            var uniquePath = GetDefaultProjectPath(groupInfo.projectPath);
            if (m_defaultProjects.ContainsKey(uniquePath))
            {
                var group = m_uiGameboard.codeManager.AddRobotToGroup(index, uniquePath);
                group.project = m_defaultProjects[uniquePath];
            }
        }

        private void SetDirtyIfGBGroupChanged()
        {
            if (m_gameboard.GetCodeGroups(ScriptLanguage.Visual) == m_userGroups)
            {
                m_uiGameboard.undoManager.SetClean(false);
            }
        }

        private void OnRobotRemoved(int robotIndex)
        {
            m_uiGameboard.codeManager.RemoveRobot(robotIndex);
        }

        private void OnRobotGroupChanged(int robotIndex, ICodeGroup group)
        {
            string newProjectPath = group != null ? group.projectPath : "";
            if (!m_defaultProjects.ContainsKey(newProjectPath) && Path.GetFileName(newProjectPath) != "")
            {
                m_userGroups.SetRobotCode(robotIndex, newProjectPath);
            }
            else
            {
                m_userGroups.SetRobotCode(robotIndex, null);
            }

            SetDirtyIfGBGroupChanged();
        }

        private void OnProjectDeleted(string name)
        {
            var group = m_uiGameboard.codeManager.GetGroup(name);
            if (group != null)
            {
                m_uiGameboard.codeManager.RemoveGroup(group);
                foreach (var index in group.robotIndices)
                {
                    SetRobotDefaultCode(index);
                }

                SetDirtyIfGBGroupChanged();
            }
        }

        public void EditCode(int robotIndex)
        {
            m_uiGameboard.StartCoroutine(OpenRobotCodingSpace(robotIndex));
        }

        IEnumerator OpenRobotCodingSpace(int robotIndex)
        {
            m_uiGameboard.ShowLoadingMask(true);
            yield return m_uiGameboard.codeManager.OpenRobotCodingSpace(robotIndex);
            m_uiGameboard.CloseLoadingMask();
        }

        public void AssignCode(int robotIndex, bool canCreateNew, Action onCodeAssigned)
        {
            PopupManager.ProjectView(projPath => {
                if (projPath.name == "")
                {
                    // new project always has a separate group
                    var group = m_uiGameboard.codeManager.CreateLocalGroup(projPath.ToString());
                    m_uiGameboard.codeManager.AddRobotToGroup(robotIndex, group);
                }
                else
                {
                    var group = m_uiGameboard.codeManager.GetOrCreateLocalGroup(projPath.ToString());
                    if (group != null)
                    {
                        m_uiGameboard.codeManager.AddRobotToGroup(robotIndex, group);
                    }
                    else
                    {
                        PopupManager.Notice("ui_failed_to_load_project".Localize());
                        return;
                    }
                }

                if (onCodeAssigned != null)
                {
                    onCodeAssigned();
                }
            }, showAddCell: canCreateNew);
        }

        public void UnassignCode(int robotIndex)
        {
            m_uiGameboard.codeManager.RemoveRobotFromGroup(robotIndex);
            SetRobotDefaultCode(robotIndex);
        }

        public bool IsCodeAssigned(int robotIndex)
        {
            return m_uiGameboard.codeManager.IsRobotInAnyGroup(robotIndex);
        }

        public bool IsUserCodeAssigned(int robotIndex)
        {
            // robot code is considered user assigned if code is not default
            var group = m_uiGameboard.codeManager.GetGroup(robotIndex);
            return group != null && !m_defaultProjects.ContainsKey(group.projectPath);
        }

        public string GetRobotCodePath(int robotIndex)
        {
            var group = m_uiGameboard.codeManager.GetGroup(robotIndex);
            return group != null ? GetOriginalProjectPath(group.projectPath) : "";
        }
    }
}
