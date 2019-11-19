using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Assertions;

public delegate void DeletingVariableHandler(string name, Action delete);

public class NodeTemplateList : MonoBehaviour
{
    public UIWorkspace m_Workspace;
	public RectTransform m_TemplateViewList;
	public ScrollRect m_Scro;

    public NodeCategoryTitleConfig m_TitleConfig;
	public Text m_TitleName;
	public RectTransform[] m_FunctionBtn;

	public GameObject m_RunTimeMask;
    public FunctionCallNode m_FuncCallTemplate;
    public FunctionCallNode m_FuncCallReturnTemplate;
    public GameObject m_CategoryContainerTemplate;

    enum HeaderButtons
    {
        AddData,
        DeleteData,
        AddMsg,
        DeleteMsg,
        AddFunction,
        Num
    }

	GameObject m_MainNode;
	private readonly Dictionary<int, List<FunctionNode>> m_NodeFilter = new Dictionary<int, List<FunctionNode>>();
    private readonly Dictionary<int, FunctionNode> m_NodeTemplates = new Dictionary<int, FunctionNode>();
    private readonly List<Canvas> m_CategoryCanvases = new List<Canvas>();

	private readonly List<FunctionNode> m_VarNode = new List<FunctionNode>();
	private readonly List<FunctionNode> m_ListNode = new List<FunctionNode>();
	private readonly List<FunctionNode> m_StackNode = new List<FunctionNode>();
	private readonly List<FunctionNode> m_QueueNode = new List<FunctionNode>();
    private int m_NumFuncTemplates;

    private NodeCategory m_CurrentCategory = NodeCategory.Count;
    private bool m_MaskEnabled;
    private NodeFilterData m_NodeFilterData;
    private Vector2 m_ViewSize;
    private FunctionDeclarationNode m_FunctionDeclNode;
    private int m_nextFuncCallTempId = -1;
    private readonly List<FunctionDeclaration> m_DirtyFuncCalls = new List<FunctionDeclaration>();
    private bool m_NodesVisible = true;

    private static readonly Vector2 s_kPadding = new Vector2(10, 10);

    private class FuncCallTemplateState
    {
        public int relativeIndex; // the relative index of the first call template for a func decl
        public int templateId; // the template id of the first call template
    }

    // Use this for initialization
    void Awake()
	{
		for (var cate = NodeCategory.Hamster; cate < NodeCategory.Count; ++cate)
		{
			m_NodeFilter[(int)cate] = new List<FunctionNode>();
            var canvas = Instantiate(m_CategoryContainerTemplate, m_CategoryContainerTemplate.transform.parent)
                            .GetComponent<Canvas>();
            m_CategoryCanvases.Add(canvas);
		}
	}

	void CategorizeDataNode(FunctionNode template)
	{
        var varBlock = template.GetComponent<VariableBaseBlock>();
        if (varBlock)
        {
            if (varBlock is VariableBaseVarBlock)
            {
                m_VarNode.Add(template);
            }
            else if (varBlock is VariableBaseListBlock)
            {
                m_ListNode.Add(template);
            }
            else if (varBlock is VariableBaseStackBlock)
            {
                m_StackNode.Add(template);
            }
            else if (varBlock is VariableBaseQueueBlock)
            {
                m_QueueNode.Add(template);
            }
        }
	}

    public NodeFilterData NodeFilterData
    {
        get { return m_NodeFilterData; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_NodeFilterData = value;
        }
    }

	public void AddNodeTemplate(int templateId, bool enabled)
	{
        var prototype = NodeTemplateCache.Instance.GetTemplate(templateId);
        var template = m_Workspace.CloneNode(prototype, m_CategoryCanvases[(int)prototype.NodeCategory].transform);
        template.IsTemplate = true;

        m_Workspace.RegisterNodeCallbacks(template);
        if (enabled)
        {
            CategorizeDataNode(template);
            if (template.NodeCategory == NodeCategory.Function)
            {
                // place all function template nodes in front of function call nodes
                FuncNodes.Insert(m_NumFuncTemplates, template);
                ++m_NumFuncTemplates;
            }
            else
            {
                m_NodeFilter[(int)template.NodeCategory].Add(template);
            }
        }
        // disabled node is still added to the template pool
        // in case any project is still using the node
		m_NodeTemplates.Add(template.NodeTemplateId, template);
        if (!m_FunctionDeclNode && template is FunctionDeclarationNode)
        {
            m_FunctionDeclNode = template as FunctionDeclarationNode;
        }
	}

	public void ShowNodeByFilter(NodeCategory type)
	{
        if (m_CurrentCategory == type || type == NodeCategory.Count)
        {
            return;
        }

        if (m_CurrentCategory != NodeCategory.Count)
        {
            m_CategoryCanvases[(int)m_CurrentCategory].enabled = false;
        }
        m_CurrentCategory = type;
        m_CategoryCanvases[(int)m_CurrentCategory].enabled = m_NodesVisible;

        RefreshCurrentCategory();
        m_Scro.normalizedPosition = new Vector2(0.0f, 1.0f);
	}

    public void RefreshCategory(NodeCategory category)
    {
        if (m_CurrentCategory == category)
        {
            RefreshCurrentCategory();
        }
    }

    public void RefreshCurrentCategory()
    {
        if (m_CurrentCategory == NodeCategory.Count)
        {
            return;
        }

        m_ViewSize = Vector2.zero;

        var headerButtons = GetHeaderButtons();
        ShowHeaders(headerButtons);
        UpdateShowNodes();

        m_ViewSize.x += 2 * s_kPadding.x;
        m_Scro.content.SetSize(m_ViewSize);

        m_TitleName.text = m_TitleConfig.GetLocId(m_CurrentCategory).Localize();
        RebuildFuncCallNodes();
    }

    private List<HeaderButtons> GetHeaderButtons()
    {
        var headerButtons = new List<HeaderButtons>();
        if (m_CurrentCategory == NodeCategory.Data)
        {
            headerButtons.Add(HeaderButtons.AddData);
            if (m_Workspace.CodeContext.variableManager.count != 0)
            {
                headerButtons.Add(HeaderButtons.DeleteData);
            }
        }
        else if (m_CurrentCategory == NodeCategory.Events && Preference.blockLevel > BlockLevel.Beginner)
        {
            headerButtons.Add(HeaderButtons.AddMsg);
            if (m_Workspace.CodeContext.messageManager.count != 0)
            {
                headerButtons.Add(HeaderButtons.DeleteMsg);
            }
        }
        else if (m_CurrentCategory == NodeCategory.Function)
        {
            headerButtons.Add(HeaderButtons.AddFunction);
        }
        return headerButtons;
    }

    void ShowHeaders(IList<HeaderButtons> buttons)
    {
        for (int i = 0; i < m_FunctionBtn.Length; ++i)
        {
            var visible = buttons.Contains((HeaderButtons)i);
            m_FunctionBtn[i].gameObject.SetActive(visible);
            if (visible)
            {
                m_FunctionBtn[i].localPosition = new Vector2(s_kPadding.x, -m_ViewSize.y);
                m_ViewSize.y += m_FunctionBtn[i].rect.height + s_kPadding.y;
                m_ViewSize.x = Mathf.Max(m_ViewSize.x, m_FunctionBtn[i].rect.width);
            }
        }
    }

    private void UpdateShowNodes()
    {
        var visibleNodes = new List<FunctionNode>();
        HashSet<FunctionNode> hiddenNodes = null;

        var currentNodes = m_NodeFilter[(int)m_CurrentCategory];
        if (m_CurrentCategory == NodeCategory.Data)
        {
            visibleNodes.AddRange(
                GetVisibleDataNodes(BlockVarType.Variable, m_VarNode)
                    .Concat(GetVisibleDataNodes(BlockVarType.List, m_ListNode))
                    .Concat(GetVisibleDataNodes(BlockVarType.Stack, m_StackNode))
                    .Concat(GetVisibleDataNodes(BlockVarType.Queue, m_QueueNode)));
        }
        else
        {
            hiddenNodes = new HashSet<FunctionNode>(currentNodes);
            var numNodes = m_CurrentCategory == NodeCategory.Function ? m_NumFuncTemplates : currentNodes.Count;
            foreach (var curNode in currentNodes.Take(numNodes))
            {
                var levelData = NodeFilterData.GetLevelData(curNode.NodeTemplateId, (int)m_Workspace.BlockLevel);
                if (levelData == null)
                {
                    continue;
                }
                visibleNodes.Add(curNode);
                hiddenNodes.Remove(curNode);
            }
        }

        var templateOrder = visibleNodes.ToDictionary(
                x => x.NodeTemplateId,
                y => NodeFilterData.GetLevelData(y.NodeTemplateId, (int)m_Workspace.BlockLevel).order);

        // reorder
        visibleNodes.Sort((x, y) => templateOrder[x.NodeTemplateId].CompareTo(templateOrder[y.NodeTemplateId]));

        if (m_CurrentCategory == NodeCategory.Function)
        {
            foreach (var node in currentNodes.Skip(m_NumFuncTemplates))
            {
                visibleNodes.Add(node);
                hiddenNodes.Remove(node);
            }
        }

        foreach (var node in visibleNodes)
        {
            node.LogicTransform.localPosition = new Vector2(s_kPadding.x, -m_ViewSize.y);
            node.gameObject.SetActive(true);

            m_ViewSize.y += node.RectTransform.rect.height + s_kPadding.y;
            m_ViewSize.x = Mathf.Max(m_ViewSize.x, node.RectTransform.rect.width);
        }

        if (hiddenNodes != null)
        {
            foreach (var node in hiddenNodes)
            {
                node.gameObject.SetActive(false);
            }
        }
    }

    IEnumerable<FunctionNode> GetVisibleDataNodes(BlockVarType type, List<FunctionNode> nodes)
	{
		bool hasVarType = m_Workspace.CodeContext.variableManager.hasVarOfType(type);
		for (int i = 0; i < nodes.Count; ++i)
		{
            var levelData = NodeFilterData.GetLevelData(nodes[i].NodeTemplateId, (int)m_Workspace.BlockLevel);
            var visible = hasVarType && levelData != null;
            nodes[i].gameObject.SetActive(visible);
            if (visible)
            {
                yield return nodes[i];
            }
		}
	}

    private void RebuildFuncCallNodes()
    {
        foreach (var decl in m_DirtyFuncCalls)
        {
            int index = IndexOfFuncCallNode(decl.functionId);
            RebuildFuncCall(index, decl);
        }
        m_DirtyFuncCalls.Clear();
    }

	public void RefreshDataNode()
	{
        RefreshCategory(NodeCategory.Data);
	}

	public void RefreshEventNode()
	{
        if (m_Workspace.BlockLevel < BlockLevel.Advanced)
        {
            return;
        }

        RefreshCategory(NodeCategory.Events);
	}

    public NodeCategory CurrentCategory
    {
        get { return m_CurrentCategory; }
    }

    public IEnumerable<FunctionNode> CurrentNodes
    {
        get
        {
            if (m_CurrentCategory == NodeCategory.Count)
            {
                return Enumerable.Empty<FunctionNode>();
            }
            return m_NodeFilter[(int)m_CurrentCategory];
        }
    }

	public FunctionNode GetTemplateByID(int id)
	{
		FunctionNode mNode = null;
		m_NodeTemplates.TryGetValue(id, out mNode);
		return mNode;
	}

    public bool ScrollEnabled
    {
        get { return m_Scro.enabled; }
        set { m_Scro.enabled = value; }
    }

	public void DragScroll(PointerEventData eventData)
	{
		eventData.pointerEnter = m_Scro.gameObject;
		eventData.pointerPress = m_Scro.gameObject;
		eventData.rawPointerPress = m_Scro.gameObject;
		eventData.pointerDrag = m_Scro.gameObject;
		m_Scro.OnBeginDrag(eventData);
	}

    public void EnableMask(bool enabled)
    {
        m_RunTimeMask.SetActive(enabled);
    }

	public void AddData()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UIAddDataDialog>();
        dialog.Configure(m_Workspace);
        dialog.OpenDialog();
	}

    public DeletingVariableHandler deletingVariableHandler
    {
        get;
        set;
    }

	public void DeleteData()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UIDeleteDataDialog>();
        dialog.Configure(m_Workspace);
        dialog.OpenDialog();
	}

	public void AddMsg()
	{
        var handler = new AddMsgPlugins(m_Workspace);
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditMessageDialog>();
        dialog.ShowGlobalFlag(CanAddGlobalData);
        dialog.ShowRobotTargets(CanAddGlobalData);
        dialog.SetRobotNum(m_Workspace.CodeContext.robotManager.robotCount);

        var config = new UIEditInputDialogConfig {
            title = "ui_dialog_message_title"
        };
        dialog.Configure(config, handler, handler);
        dialog.OpenDialog();
	}

	public void DeleteMsg()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UIDeleteMsgDialog>();
        dialog.Configure(m_Workspace);
        dialog.OpenDialog();
	}

    public bool CanAddGlobalData
    {
        get;
        set;
    }

    public void AddFunction()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditFunctionDialog>();
        dialog.Configure(m_Workspace);
        dialog.OpenDialog();
    }

    private List<FunctionNode> FuncNodes
    {
        get { return m_NodeFilter[(int)NodeCategory.Function]; }
    }

    public int NextFuncCallTemplateId
    {
        get { return m_nextFuncCallTempId; }
        set
        {
#if UNITY_EDITOR
            if (value >= FuncNodes.Select(x => x.NodeIndex).DefaultIfEmpty().Min())
            {
                throw new ArgumentOutOfRangeException("value");
            }
#endif
            m_nextFuncCallTempId = value;
        }
    }

    // Add a declaration with the given state
    public void AddFuncCall(FunctionDeclaration declaration, object state = null)
    {
        if (declaration == null)
        {
            throw new ArgumentNullException("declaration");
        }

        var declState = state as FuncCallTemplateState;
        if (state != null && declState == null)
        {
            throw new ArgumentException("invalid state");
        }

        int tempId0, tempId1;
        int index0;
        if (declState != null)
        {
            tempId0 = declState.templateId;
            tempId1 = declState.templateId - 1;
            index0 = declState.relativeIndex + m_NumFuncTemplates;
        }
        else
        {
            tempId0 = m_nextFuncCallTempId--;
            tempId1 = m_nextFuncCallTempId--;
            index0 = FuncNodes.Count;
        }
        AddFuncCall(m_FuncCallTemplate, declaration, tempId0, index0);
        AddFuncCall(m_FuncCallReturnTemplate, declaration, tempId1, index0 + 1);
    }

    private void AddFuncCall(FunctionCallNode template, FunctionDeclaration declaration, int templateId, int index)
    {
        var callNode = Instantiate(template, m_CategoryCanvases[(int)NodeCategory.Function].transform, false)
                        .GetComponent<FunctionCallNode>();
        // template id is negative for function calls because they are dynamic
        callNode.NodeTemplateId = templateId;
        callNode.IsTemplate = true;
        callNode.CodeContext = m_Workspace.CodeContext;
        callNode.NodeCategory = NodeCategory.Function;
        callNode.Rebuild(declaration);
        m_Workspace.RegisterNodeCallbacks(callNode);
        FuncNodes.Insert(index, callNode);
        m_NodeTemplates.Add(callNode.NodeTemplateId, callNode);
    }

    // return the state of the func call templates which can be passed to AddFuncCall 
    // to restore the template positions in the list
    // NOTE: this is for undo use only
    public object GetFuncCallState(Guid functionId)
    {
        int index = IndexOfFuncCallNode(functionId);
        if (index == -1)
        {
            throw new ArgumentException("invalid function id");
        }
        return new FuncCallTemplateState {
            templateId = FuncNodes[index].NodeTemplateId,
            relativeIndex = index - m_NumFuncTemplates
        };
    }

    private int IndexOfFuncCallNode(Guid functionId)
    {
        return FuncNodes.FindIndex(x => {
            if (x is FunctionCallNode)
            {
                return (x as FunctionCallNode).Declaration.functionId == functionId;
            }
            return false;
        });
    }

    public void RemoveFuncCall(Guid functionId)
    {
        var index = IndexOfFuncCallNode(functionId);
        if (index != -1)
        {
            // remove the two call templates
            RemoveFuncCallNodeAt(index);
            RemoveFuncCallNodeAt(index);
            m_DirtyFuncCalls.Remove(x => x.functionId == functionId);
        }
    }

    private void RemoveFuncCallNodeAt(int index)
    {
        var node = (FunctionCallNode)FuncNodes[index];
        Destroy(node.gameObject);
        m_NodeTemplates.Remove(node.NodeTemplateId);
        FuncNodes.RemoveAt(index);
    }

    public void RemoveAllFuncCalls()
    {
        for (int i = FuncNodes.Count - 1; i >= m_NumFuncTemplates; --i)
        {
            if (FuncNodes[i] is FunctionCallNode)
            {
                RemoveFuncCallNodeAt(i);
            }
        }
        m_DirtyFuncCalls.Clear();
    }

    private FunctionCallNode GetFuncCallNode(Guid functionId)
    {
        var index = IndexOfFuncCallNode(functionId);
        return index != -1 ? (FunctionCallNode)FuncNodes[index] : null;
    }

    public void RebuildFuncCall(FunctionDeclaration decl)
    {
        if (decl == null)
        {
            throw new ArgumentNullException("decl");
        }

        var index = IndexOfFuncCallNode(decl.functionId);
        if (index == -1)
        {
            throw new ArgumentException("invalid function id", "decl");
        }

        if (m_CurrentCategory == NodeCategory.Function)
        {
            RebuildFuncCall(index, decl);
        }
        else
        {
            // delay the rebuild until the node is visible
            m_DirtyFuncCalls.Remove(x => x.functionId == decl.functionId);
            m_DirtyFuncCalls.Add(decl);
        }
    }

    private void RebuildFuncCall(int firstTempIndex, FunctionDeclaration decl)
    {
        (FuncNodes[firstTempIndex] as FunctionCallNode).Rebuild(decl);
        (FuncNodes[firstTempIndex + 1] as FunctionCallNode).Rebuild(decl);
    }

    public FunctionDeclarationNode FuncDeclNode
    {
        get { return m_FunctionDeclNode; }
    }

    public void ShowNodes(bool visible)
    {
        m_NodesVisible = visible;
        if (m_CurrentCategory != NodeCategory.Count)
        {
            m_CategoryCanvases[(int)m_CurrentCategory].enabled = m_NodesVisible;
        }
    }
}
