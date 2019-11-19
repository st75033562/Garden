using DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Scheduling;

namespace Gameboard
{
    public interface ICodeGroup
    {
        string projectPath { get; }

        string workingDirectory { get; set; }

        IEnumerable<int> robotIndices { get; }

        IEnumerator Refresh();

        /// <summary>
        /// isProjectDirty is set to true after updating the project
        /// </summary>
        Project project { get; set; }

        bool isProjectDirty { get; set; }
    }

    /// <summary>
    /// <para>manage robot grouping and corresponding UIWorkspaces</para>
    /// <para>a robot can only be in a group at a time</para>
    /// </summary>
    public class RobotCodeManager : MonoBehaviour
    {
        public event Action<UIWorkspace> onWorkspaceOpened;
        public event Action<UIWorkspace> onWorkspaceClosed;

        public event Action<int, ICodeGroup> onRobotGroupChanged;

        [SerializeField] UIWorkspace workspaceTemplate;

        private class CodeGroupImpl : ICodeGroup
        {
            public UIWorkspace workspace; // workspace for running code

            private Project m_project;
            public readonly RobotCollection robots;

            public bool needRefreshRobots = true;
            public int id; // for debugging

            private readonly RobotCodeManager m_manager;
            private string m_workingDirectory = "";
            private string m_projectPath;

            public CodeGroupImpl(RobotCodeManager manager, string path)
            {
                m_manager = manager;
                robots = new RobotCollection();
                projectPath = path;
                isProjectDirty = true;
            }

            public Project project
            {
                get { return m_project; }
                set
                {
                    m_project = value;
                    isProjectDirty = true;
                }
            }

            public bool isProjectDirty
            {
                get;
                set;
            }

            public IEnumerator Refresh()
            {
                if (!workspace)
                {
                    var request = Scheduler.instance.Schedule<UIWorkspace>(m_manager.CreateRobotWorkspace());
                    yield return request;
                    workspace = request.result;
                    workspace.WorkingDirectory = workingDirectory;
                }

                if (needRefreshRobots)
                {
                    workspace.CodeContext.robotManager = robots;
                    needRefreshRobots = false;
                }

                if (projectPathChanged)
                {
                    workspace.Id = projectPath;
                }

                if (isProjectDirty)
                {
                    isProjectDirty = false;
                    yield return workspace.Load(project);
                }
            }

            public void OnRobotRemoved(int robotIndex)
            {
                robots.OnRobotRemoved(robotIndex);
            }

            public void RemoveRobots()
            {
                robots.RemoveAll();
            }

            public void AddRobot(int index)
            {
                robots.Add(index);
                needRefreshRobots = true;
            }

            public void RemoveRobot(int index)
            {
                robots.Remove(index);
                needRefreshRobots = true;
            }

            public bool ContainsRobot(int index)
            {
                return robots.Contains(index);
            }

            public int robotCount
            {
                get { return robots.robotCount; }
            }

            public string projectPath
            {
                get { return m_projectPath; }
                set
                {
                    value = value ?? "";
                    if (m_projectPath != value)
                    {
                        m_projectPath = value;
                        projectPathChanged = true;
                    }
                }
            }

            public bool projectPathChanged
            {
                get;
                private set;
            }

            public string workingDirectory
            {
                get { return m_workingDirectory; }
                set
                {
                    if (m_workingDirectory == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    m_workingDirectory = value;
                    if (workspace)
                    {
                        workspace.WorkingDirectory = m_workingDirectory;
                    }
                }
            }

            public IEnumerable<int> robotIndices
            {
                get { return robots.robotIndices; }
            }

            public override string ToString()
            {
                return string.Format("{0}: {1}", id, projectPath);
            }
        }

        private readonly List<CodeGroupImpl> m_codeGroups = new List<CodeGroupImpl>();
        private readonly Stack<UIWorkspace> m_workspacePool = new Stack<UIWorkspace>();

        private int m_baseSortingOrder;
        private IRobotManager m_robotManager;

        private UIWorkspace m_activeWorkspace;
        private int m_activeRobotIndex = -1; // active robot index for editing code

        private CodeProjectRepository m_repo;

        // for debugging
        private int m_nextId;

        public const string GlobalDataPrefix = "gb.";

        private readonly GlobalMessageForwarder m_globalMessageForwarder = new GlobalMessageForwarder(GlobalDataPrefix);
        private GlobalMemory m_globalMemory;

        private MessageManager m_gameboardMessageManager;
        private VariableManager m_gameboardVariableManager;
        private ARSceneManager m_arSceneManager;
        private Func<ISceneView> m_sceneViewFactory;
        private IGameboardPlayer m_gameboardPlayer;
        private VisualScriptController m_scriptController;
        private bool m_showSubmitButton;
        private bool m_prepared;

        public void Initialize(int baseSortingOrder, 
                               CodeProjectRepository repo, 
                               UIWorkspace gameboardWorkspace,
                               ARSceneManager arSceneManager,
                               IGameboardPlayer gameboardPlayer,
                               Func<ISceneView> sceneViewFactory,
                               VisualScriptController scriptController,
                               bool submitButtonVisible)
        {
            m_baseSortingOrder = baseSortingOrder;
            m_repo = repo;
            m_arSceneManager = arSceneManager;
            m_sceneViewFactory = sceneViewFactory;
            m_gameboardPlayer = gameboardPlayer;
            m_scriptController = scriptController;
            m_showSubmitButton = submitButtonVisible;

            m_gameboardMessageManager = gameboardWorkspace.CodeContext.messageManager;
            m_gameboardVariableManager = gameboardWorkspace.CodeContext.variableManager;
            m_globalMemory = new GlobalMemory(GlobalDataPrefix, m_gameboardVariableManager);

            gameboardWorkspace.OnBeforeLoadingCode += OnBeforeLoadingGameboardCode;
            gameboardWorkspace.OnDidLoadCode += OnDidLoadGameboardCode;
            m_gameboardMessageManager.addListener(m_globalMessageForwarder);
        }

        public void RemoveRobot(int robotIndex)
        {
            RemoveRobotFromGroup(robotIndex);
            foreach (var group in m_codeGroups)
            {
                group.OnRobotRemoved(robotIndex);
            }
        }

        private void OnBeforeLoadingGameboardCode(UIWorkspace workspace)
        {
            m_gameboardMessageManager.onMessageAdded -= OnGameboardMessageAdded;
            m_gameboardVariableManager.onVariableAdded.RemoveListener(OnGameboardVariableAdded);
        }

        private void OnDidLoadGameboardCode(UIWorkspace workspace)
        {
            m_gameboardMessageManager.onMessageAdded += OnGameboardMessageAdded;
            m_gameboardVariableManager.onVariableAdded.AddListener(OnGameboardVariableAdded);

            RefreshAllWorkspaceGlobalData();
        }

        void OnDestroy()
        {
            RemoveAllGroups();
            DestroyPooledWorkspaces();
        }

        public int activeRobotIndex
        {
            get { return m_activeRobotIndex; }
        }

        public void SetCodeGroups(RobotCodeGroups codeGroups)
        {
            RemoveAllGroups();
        }

        public void SetRobotManager(IRobotManager robotManager)
        {
            if (robotManager == null)
            {
                throw new ArgumentNullException();
            }

            m_robotManager = robotManager;
            foreach (var group in m_codeGroups)
            {
                group.robots.robotManager = robotManager;
            }
        }

        /// <summary>
        /// open the workspace for editing robot code, for now only one workspace can be opened at a time
        /// </summary>
        public IEnumerator OpenRobotCodingSpace(int robotIndex)
        {
            if (m_activeRobotIndex != -1 && m_activeRobotIndex != robotIndex)
            {
                throw new InvalidOperationException();
            }

            var group = (CodeGroupImpl)GetGroup(robotIndex);
            if (group == null)
            {
                throw new ArgumentException();
            }

            yield return group.Refresh();

            m_activeRobotIndex = robotIndex;
            m_activeWorkspace = group.workspace;
            OpenWorkspace(group.workspace);
        }

        public void CloseRobotCodingSpace(bool wasSaved)
        {
            if (!m_activeWorkspace)
            {
                return;
            }

            if (!wasSaved)
            {
                var group = (CodeGroupImpl)GetGroup(m_activeRobotIndex);
                if (group != null)
                {
                    group.isProjectDirty = true;
                    group.needRefreshRobots = true;
                }
            }

            CloseWorkspace(m_activeWorkspace);

            if (!m_codeGroups.Any(x => x.workspace == m_activeWorkspace))
            {
                m_workspacePool.Push(m_activeWorkspace);
            }
            m_activeWorkspace = null;
            m_activeRobotIndex = -1;
        }

        public IEnumerator PrepareRunning()
        {
            yield return RefreshGroupWorkspaces();
            foreach (var group in m_codeGroups)
            {
                group.workspace.IsReadOnly = true;
            }
            m_prepared = true;
        }

        // Run all the robot code, PrepareRunning must already be called
        public void Run()
        {
            if (!m_prepared)
            {
                throw new InvalidOperationException("PrepareRunning not called");
            }

            m_globalMessageForwarder.RemoveReceivers();

            for (int i = 0; i < m_codeGroups.Count; ++i)
            {
                var group = m_codeGroups[i];
                m_globalMessageForwarder.AddReceiver(i, group.workspace.CodeContext.messageManager);
                group.workspace.Run(false, true);
            }
        }

        public void Stop()
        {
            m_prepared = false;
            foreach (var group in m_codeGroups)
            {
                group.workspace.Stop();
                group.workspace.IsReadOnly = false;
            }
        }

        public void SetPaused(bool paused)
        {
            foreach (var group in m_codeGroups)
            {
                group.workspace.SetPaused(paused);
            }
        }

        public bool IsRobotInAnyGroup(int robotIndex)
        {
            return GetGroup(robotIndex) != null;
        }

        public ICodeGroup GetGroup(int robotIndex)
        {
            return m_codeGroups.Find(x => x.ContainsRobot(robotIndex));
        }

        private CodeGroupImpl CreateGroupInternal(string path)
        {
            var group = new CodeGroupImpl(this, path);
            group.robots.robotManager = m_robotManager;
            group.id = m_nextId++;

            m_codeGroups.Add(group);
            return group;
        }

        public ICodeGroup CreateGroup(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return CreateGroupInternal(path);
        }

        public ICodeGroup GetGroup(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return m_codeGroups.Find(x => x.projectPath == path);
        }

        public ICodeGroup GetOrCreateGroup(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var group = m_codeGroups.Find(x => x.projectPath == path);
            if (group != null)
            {
                return group;
            }

            return CreateGroupInternal(path);
        }

        // a local group is simply a group with local path
        // if project name is not empty, the code is loaded, and working directory is updated
        // if code is failed to load, return null
        public ICodeGroup CreateLocalGroup(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            Project code = null;
            if (Path.GetFileName(path) != "")
            {
                code = m_repo.loadCodeProject(path);
                if (code == null)
                {
                    return null;
                }
            }

            var group = CreateGroupInternal(path);
            group.workingDirectory = path != "" ? Path.GetDirectoryName(path) : "";
            if (code != null)
            {
                group.project = code;
            }
            return group;
        }

        public ICodeGroup GetOrCreateLocalGroup(string path)
        {
            var group = GetGroup(path);
            if (group != null)
            {
                return group;
            }

            return CreateLocalGroup(path);
        }

        public ICodeGroup AddRobotToGroup(int robotIndex, string path)
        {
            var codeGroup = GetOrCreateGroup(path);
            AddRobotToGroup(robotIndex, codeGroup);
            return codeGroup;
        }

        public void AddRobotToGroup(int robotIndex, ICodeGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            var oldGroup = (CodeGroupImpl)GetGroup(robotIndex);
            if (oldGroup != null)
            {
                if (oldGroup == group)
                {
                    return;
                }

                RemoveRobotFromGroup(robotIndex, oldGroup);
            }

            var targetGroup = group as CodeGroupImpl;
            targetGroup.AddRobot(robotIndex);
            if (onRobotGroupChanged != null)
            {
                onRobotGroupChanged(robotIndex, targetGroup);
            }

            Debug.LogFormat("added robot {0} to group {1}", robotIndex, targetGroup);

            if (m_activeWorkspace && m_activeRobotIndex == robotIndex)
            {
                // invalidate the old group's workspace, since the workspace will be migrated
                if (oldGroup != null && oldGroup.workspace == m_activeWorkspace)
                {
                    oldGroup.workspace = null;
                    oldGroup.isProjectDirty = true;
                }

                if (targetGroup.workspace)
                {
                    // recycle the existing workspace
                    m_workspacePool.Push(targetGroup.workspace);
                }
                targetGroup.workspace = m_activeWorkspace;
                targetGroup.isProjectDirty = true;
            }
        }

        public void RemoveRobotFromGroup(int robotIndex)
        {
            var group = (CodeGroupImpl)GetGroup(robotIndex);
            if (group != null)
            {
                RemoveRobotFromGroup(robotIndex, group);
                if (onRobotGroupChanged != null)
                {
                    onRobotGroupChanged(robotIndex, null);
                }
            }
        }

        private void RemoveRobotFromGroup(int robotIndex, CodeGroupImpl group)
        {
            Debug.LogFormat("remove robot {0} from group {1}", robotIndex, group);

            group.RemoveRobot(robotIndex);

            if (group.robotCount == 0)
            {
                RemoveGroup(group);
            }
        }

        public void RemoveAllGroups()
        {
            foreach (var group in m_codeGroups)
            {
                if (group.workspace != null)
                {
                    m_workspacePool.Push(group.workspace);
                }
            }
            m_codeGroups.Clear();
        }

        public void RemoveGroup(ICodeGroup group)
        {
            var targetGroup = group as CodeGroupImpl;
            // do not recycle the active workspace, otherwise it will be deleted
            if (targetGroup.workspace != m_activeWorkspace)
            {
                m_workspacePool.Push(targetGroup.workspace);
            }
            m_codeGroups.Remove(targetGroup);

            if (onRobotGroupChanged != null)
            {
                foreach (var index in targetGroup.robotIndices)
                {
                    onRobotGroupChanged(index, null);       
                }
            }

            Debug.LogFormat("removed group {0}", targetGroup);
        }

        private IEnumerator CreateRobotWorkspace()
        {
            if (m_workspacePool.Count > 0)
            {
                var workspace = m_workspacePool.Pop();
                workspace.ResetLeftPanel();
                yield return workspace;
            }
            else
            {
                var workspace = Instantiate(workspaceTemplate.gameObject).GetComponent<UIWorkspace>();
                workspace.m_RootCanvas.sortingOrder = m_baseSortingOrder;
                workspace.Show(false);
                yield return workspace.Init(
                    VoiceRepository.instance, 
                    NodeFilterData.GetFilter(NodeFilterType.Robot), 
                    false, 
                    m_sceneViewFactory());
                workspace.OnDidLoadCode += RefreshGlobalData;
                workspace.CodeContext.arSceneManager = m_arSceneManager;

                m_globalMemory.AddClient(workspace.CodeContext.variableManager);

                var controller = workspace.GetComponent<GameboardRobotCodeController>();
                controller.Init(this, m_gameboardPlayer, m_scriptController);
                controller.ShowSubmitButton(m_showSubmitButton);

                yield return workspace;
            }
        }

        private void OnGameboardMessageAdded(Message msg)
        {
            RefreshAllWorkspaceGlobalData();
        }

        private void OnGameboardVariableAdded(BaseVariable variable)
        {
            if (variable.scope == NameScope.Global)
            {
                RefreshAllWorkspaceGlobalData();
            }
        }

        private void RefreshAllWorkspaceGlobalData()
        {
            foreach (var group in m_codeGroups)
            {
                if (group.workspace)
                {
                    RefreshGlobalData(group.workspace);
                }
            }
        }

        private void RefreshGlobalData(UIWorkspace workspace)
        {
            AddGlobalMessages(workspace);
            AddGlobalVariables(workspace);
        }
        
        private void AddGlobalMessages(UIWorkspace workspace)
        {
            var newGlobalMsgs = m_gameboardMessageManager.globalMessages
                .Select(x => GlobalDataPrefix + x.name)
                .Where(x => !workspace.CodeContext.messageManager.has(x))
                .Select(x => new Message(x, NameScope.Global))
                .ToArray();

            if (newGlobalMsgs.Length > 0)
            {
                workspace.UndoManager.AddUndo(new AddMessageCommand(workspace, newGlobalMsgs));
            }
        }

        private void AddGlobalVariables(UIWorkspace workspace)
        {
            workspace.UndoManager.BeginMacro();

            foreach (var variable in m_gameboardVariableManager)
            {
                if (variable.scope == NameScope.Global)
                {
                    string globalName = GlobalDataPrefix + variable.name;
                    var globalVar = workspace.CodeContext.variableManager.get(globalName);
                    if (globalVar == null)
                    {
                        var cmd = new AddVariablesCommand(workspace, new[] { variable.clone(globalName) });
                        workspace.UndoManager.AddUndo(cmd);
                    }
                    else if (globalVar.globalVarOwner != variable.globalVarOwner)
                    {
                        var oldOwner = variable.globalVarOwner;
                        var newOwner = globalVar.globalVarOwner;

                        var cmd = new SimpleUndoCommand(() => variable.globalVarOwner = oldOwner,
                                                        () => variable.globalVarOwner = newOwner);
                        workspace.UndoManager.AddUndo(cmd);
                    }
                }
            }

            workspace.UndoManager.EndMacro();
        }

        private void OpenWorkspace(UIWorkspace workspace)
        {
            workspace.Show(true);
            if (onWorkspaceOpened != null)
            {
                onWorkspaceOpened(workspace);
            }
        }

        private void CloseWorkspace(UIWorkspace workspace)
        {
            workspace.Show(false);
            if (onWorkspaceClosed != null)
            {
                onWorkspaceClosed(workspace);
            }
        }

        private IEnumerator RefreshGroupWorkspaces()
        {
            yield return Scheduler.instance.Schedule(m_codeGroups.Select(x => x.Refresh()));
        }

        private void Update()
        {
            DestroyPooledWorkspaces();
        }

        private void DestroyPooledWorkspaces()
        {
            while (m_workspacePool.Count > 0)
            {
                var workspace = m_workspacePool.Pop();
                if (workspace)
                {
                    Destroy(workspace.gameObject);

                    m_globalMessageForwarder.RemoveReceiver(workspace.CodeContext.messageManager);
                    m_globalMemory.RemoveClient(workspace.CodeContext.variableManager);
                }
            }
        }
    }
}
