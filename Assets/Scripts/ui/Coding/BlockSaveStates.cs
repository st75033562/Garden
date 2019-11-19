using System;
using System.Collections.Generic;
using System.Linq;

public class BlockSaveStates
{
    public class DeclNodeInfo
    {
        public DeclNodeInfo(FunctionDeclaration decl, int nodeIndex = 0)
        {
            this.decl = decl;
            this.nodeIndex = nodeIndex;
            this.resolved = nodeIndex != 0;
        }

        public DeclNodeInfo(DeclNodeInfo rhs)
        {
            decl = new FunctionDeclaration(rhs.decl);
            nodeIndex = rhs.nodeIndex;
            resolved = rhs.resolved;
        }

        public FunctionDeclaration decl { get; private set; }
        public int nodeIndex { get; internal set; }
        public bool resolved { get; internal set; }
    }

    private class State
    {
        public readonly List<Save_NodeData> nodeStates = new List<Save_NodeData>();
        // node index -> index into m_funcDecls
        public readonly Dictionary<int, int> callNodes;
        public readonly Dictionary<int, int> callReturnNodes;

        public readonly HashSet<Message> referencedMessages = new HashSet<Message>();
        public readonly HashSet<BaseVariable> referencedVariabls = new HashSet<BaseVariable>();

        public State()
        {
            callNodes = new Dictionary<int, int>();
            callReturnNodes = new Dictionary<int, int>();
        }

        public State(IEnumerable<Save_NodeData> nodeStates, IDictionary<int, int> callNodes, IDictionary<int, int> callReturnNodes)
        {
            this.nodeStates.AddRange(nodeStates);
            this.callNodes = new Dictionary<int,int>(callNodes);
            this.callReturnNodes = new Dictionary<int,int>(callReturnNodes);
        }

        public Dictionary<int, int> GetCallDict(bool callReturn)
        {
            return callReturn ? callReturnNodes : callNodes;
        }
    }

    private bool m_includeUnresolvedFuncs;
    // states of bodies of unresolved functions
    private readonly List<DeclNodeInfo> m_funcDecls = new List<DeclNodeInfo>();
    private readonly State m_resolvedState;
    private readonly State m_unresolvedState = new State();

    /// <summary>
    /// Save states of all nodes in each block chain
    /// NOTE: There should be no intersection in all the given block chains
    /// </summary>
    /// <param name="topNodes">sequence of block chain header</param>
    public BlockSaveStates(UIWorkspace workspace, IEnumerable<FunctionNode> headNodes, bool includeUnresolvedFuncs)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }
        if (headNodes == null)
        {
            throw new ArgumentNullException("headNodes");
        }

        m_includeUnresolvedFuncs = includeUnresolvedFuncs;
        m_resolvedState = new State();

        SaveTo(workspace, headNodes.SelectMany(x => NodeUtils.GetDescendants(x, x.GetLastNode())), m_resolvedState);

        // save bodies of all unresolved functions
        if (includeUnresolvedFuncs)
        {
            SaveUnresolvedFuncs(workspace);
        }
    }

    private void SaveTo(UIWorkspace workspace, IEnumerable<FunctionNode> nodes, State state)
    {
        foreach (var node in nodes)
        {
            if (node.IsTransient)
            {
                continue;
            }

            foreach (var plugin in node.Plugins)
            {
                if (plugin is DataMenuPlugins)
                {
                    var variable = workspace.CodeContext.variableManager.get(plugin.GetPluginsText());
                    if (variable != null)
                    {
                        state.referencedVariabls.Add(variable);
                    }
                }
                else if (plugin is MessageMenuPlugins)
                {
                    var message = workspace.CodeContext.messageManager.get(plugin.GetPluginsText());
                    if (message != null)
                    {
                        state.referencedMessages.Add(message);
                    }
                }
            }

            state.nodeStates.Add(node.GetNodeSaveData());
            if (node is FunctionCallNode)
            {
                var callNode = node as FunctionCallNode;
                state.GetCallDict(node.Insertable).Add(node.NodeIndex, AddFuncDecl(0, callNode.Declaration));
            }
            else if (node is FunctionDeclarationNode)
            {
                var declNode = node as FunctionDeclarationNode;
                AddFuncDecl(node.NodeIndex, declNode.Declaration);
            }
        }
    }

    private void SaveUnresolvedFuncs(UIWorkspace workspace)
    {
        var codePanel = workspace.CodePanel;
        foreach (var func in m_funcDecls)
        {
            if (func.resolved)
            {
                continue;
            }

            var declNode = codePanel.GetFunctionNode(func.decl.functionId);
            func.nodeIndex = declNode.NodeIndex;

            var funcNodes = NodeUtils.GetDescendants(declNode, declNode.GetLastNode());
            m_unresolvedState.nodeStates.Add(funcNodes.First().GetNodeSaveData());
            SaveTo(workspace, funcNodes.Skip(1), m_unresolvedState);
        }
    }

    private int AddFuncDecl(int nodeIndex, FunctionDeclaration decl)
    {
        var index = m_funcDecls.FindIndex(x => x.decl.functionId == decl.functionId);
        if (index == -1)
        {
            m_funcDecls.Add(new DeclNodeInfo(decl, nodeIndex));
            return m_funcDecls.Count - 1;
        }
        else
        {
            var info = m_funcDecls[index];
            if (info.nodeIndex == 0 && nodeIndex != 0)
            {
                info.nodeIndex = nodeIndex;
                info.resolved = true;
            }
            return index;
        }
    }

    public BlockSaveStates(
        IEnumerable<Save_NodeData> states, 
        IEnumerable<FunctionDeclaration> decls, 
        IDictionary<int, int> declNodes,
        IDictionary<int, int> callNodes,
        IDictionary<int, int> callReturnNodes)
    {
        if (states == null)
        {
            throw new ArgumentNullException("states");
        }
        if (decls == null)
        {
            throw new ArgumentNullException("decls");
        }
        if (declNodes == null)
        {
            throw new ArgumentNullException("declNodes");
        }
        if (callNodes == null)
        {
            throw new ArgumentNullException("callNodes");
        }
        if (callReturnNodes == null)
        {
            throw new ArgumentNullException("callReturnNodes");
        }

        m_includeUnresolvedFuncs = true;
        m_funcDecls.AddRange(decls.Select(x => new DeclNodeInfo(x)));
        foreach (var kv in declNodes)
        {
            var info = m_funcDecls[kv.Value];
            info.nodeIndex = kv.Key;
            info.resolved = true;
        }
        m_resolvedState = new State(states, callNodes, callReturnNodes);
    }

    private BlockSaveStates(BlockSaveStates rhs, bool includeUnresolved)
    {
        m_includeUnresolvedFuncs = includeUnresolved;
        foreach (var info in rhs.m_funcDecls)
        {
            var copy = new DeclNodeInfo(info);
            m_funcDecls.Add(copy);
            if (includeUnresolved || info.resolved)
            {
                copy.decl.functionId = Guid.NewGuid();
            }
        }

        m_resolvedState = rhs.m_resolvedState;
        m_unresolvedState = rhs.m_unresolvedState;
    }

    public bool isEmpty
    {
        get { return m_resolvedState.nodeStates.Count == 0; }
    }

    public int stateCount
    {
        get
        {
            if (m_includeUnresolvedFuncs)
            {
                return m_resolvedState.nodeStates.Count + m_unresolvedState.nodeStates.Count;
            }
            else
            {
                return m_resolvedState.nodeStates.Count;
            }
        }
    }

    public IEnumerable<Save_NodeData> GetNodeStates()
    {
        if (m_includeUnresolvedFuncs)
        {
            return m_resolvedState.nodeStates.Concat(m_unresolvedState.nodeStates);
        }
        else
        {
            return m_resolvedState.nodeStates;
        }
    }

    public IEnumerable<DeclNodeInfo> GetFuncDecls()
    {
        foreach (var info in m_funcDecls)
        {
            if (m_includeUnresolvedFuncs || info.resolved)
            {
                yield return info;
            }
        }
    }

    public IEnumerable<KeyValuePair<int, FunctionDeclaration>> funcCalls
    {
        get
        {
            return GetCalls(false);
        }
    }

    public IEnumerable<KeyValuePair<int, FunctionDeclaration>> funcReturnCalls
    {
        get
        {
            return GetCalls(true);
        }
    }

    private IEnumerable<KeyValuePair<int, FunctionDeclaration>> GetCalls(bool callReturn)
    {
        var nodes = m_resolvedState.GetCallDict(callReturn).AsEnumerable();
        if (m_includeUnresolvedFuncs)
        {
            nodes = nodes.Concat(m_unresolvedState.GetCallDict(callReturn));
        }
        foreach (var kv in nodes)
        {
            var declInfo = m_funcDecls[kv.Value];
            yield return new KeyValuePair<int, FunctionDeclaration>(kv.Key, declInfo.decl);
        }
    }

    public IEnumerable<Message> referencedMessages
    {
        get
        {
            foreach (var msg in m_resolvedState.referencedMessages)
            {
                yield return msg;
            }
            if (m_includeUnresolvedFuncs)
            {
                foreach (var msg in m_unresolvedState.referencedMessages)
                {
                    if (!m_resolvedState.referencedMessages.Contains(msg))
                    {
                        yield return msg;
                    }
                }
            }
        }
    }

    public IEnumerable<BaseVariable> referencedVariables
    {
        get
        {
            foreach (var variable in m_resolvedState.referencedVariabls)
            {
                yield return variable;
            }
            if (m_includeUnresolvedFuncs)
            {
                foreach (var variable in m_unresolvedState.referencedVariabls)
                {
                    if (!m_resolvedState.referencedVariabls.Contains(variable))
                    {
                        yield return variable;
                    }
                }
            }
        }
    }

    /// <summary>
    /// generate a new id for each function, return the new state
    /// </summary>
    public BlockSaveStates MakeNewFunctions(bool includeUnresolvedFuncs)
    {
        return new BlockSaveStates(this, includeUnresolvedFuncs);
    }
}
