using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageKeyChangedEvent
{
    public string m_OldKey;
    public string m_NewKey;
}

public class MessageKeyChangedData
{
    public LeaveMessageList oldMsgList;
    public string newKey;
}

public class DeleteMessagesResult
{
    public readonly List<LeaveMessageList> deletedMsgLists = new List<LeaveMessageList>();
    public readonly List<MessageKeyChangedData> changedMessages = new List<MessageKeyChangedData>();
}

public class LeaveMessagePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] BoolUnityEvent m_OnActivating;
    [SerializeField] BoolUnityEvent m_OnActivated;

    public UIWorkspace m_Workspace;
    public EventBus m_EventManager;

    public ScrollRect m_ScrollView;
    public InputField m_Input;
    public GameObject m_TagTemplate;
    public RectTransform m_ChoicePanel;
    public RectTransform m_MessageContainer;

    public GameObject m_MonitorBtn;
    public GameObject m_TextInputPanel;
    public GameObject m_LeftMask;
    public RectTransform m_DeleteArea;
    public UIRecordVoice m_RecordVoice;
    public Button m_SaveButton;

    public GameObject m_MaskImage;
    public MessageListView m_MessageListView;

    List<FunctionNode> m_SelectedNodes = new List<FunctionNode>();
    Dictionary<int, MessageTag> m_TagGroup = new Dictionary<int, MessageTag>();
    LeaveMessageDataSource m_MessageDataSource = new LeaveMessageDataSource();
    float m_MergeDistance;

    bool m_NeedLayout;
    MessageTag m_LastClickTag;
    IAvatarService m_AvatarService;

    UIInputContext m_InputContext;

    void Awake()
    {
        m_EventManager.AddListener(EventId.PickUpNode, NodePickUpEventCallBack);
        m_MergeDistance = m_TagTemplate.GetComponent<RectTransform>().rect.height * 4 / 5;

        m_Input.onValueChanged.AddListener(OnMessageChanged);
        OnMessageChanged(string.Empty);

        m_InputContext = GetComponent<UIInputContext>();
        m_InputContext.enabled = false;

        m_Workspace.OnDidLoadCode += delegate {
            m_Workspace.CodePanel.OnNodeAdded += OnNodeAdded;
        };
    }

    private void OnNodeAdded(FunctionNode node)
    {
        if (IsActive)
        {
            node.EnableMessageStatus(true);
        }
    }

    private void OnMessageChanged(string text)
    {
        m_SaveButton.interactable = text.Trim() != string.Empty;
    }

    void OnDestroy()
    {
        m_EventManager.RemoveListener(EventId.PickUpNode, NodePickUpEventCallBack);
    }

    void LateUpdate()
    {
        if (m_NeedLayout)
        {
            Layout();
        }
    }

    public BoolUnityEvent OnActivating { get { return m_OnActivating; } }

    public BoolUnityEvent OnActivated { get { return m_OnActivated; } }

    public CodePanel CodePanel { get { return m_Workspace.CodePanel; } }

    public VoiceRepository VoiceRepo { get; set; }

    public LeaveMessageDataSource MessageDataSource { get { return m_MessageDataSource; } }

    public void SetMessageListNormalizedWidth(float width)
    {
        m_LeftMask.GetComponent<RectTransform>().SetAnchorMax(RectTransform.Axis.Horizontal, width);
        m_DeleteArea.SetAnchorMin(RectTransform.Axis.Horizontal, width);
    }

    public bool IsReadOnly
    {
        get { return m_MessageListView.IsReadOnly; }
        set { m_MessageListView.IsReadOnly = value; }
    }

    public IAvatarService AvatarService
    {
        get { return m_AvatarService; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_AvatarService = value;
            m_MessageListView.AvatarService = value;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.pointerEnter = m_ScrollView.gameObject;
        eventData.pointerPress = m_ScrollView.gameObject;
        eventData.rawPointerPress = m_ScrollView.gameObject;
        eventData.pointerDrag = m_ScrollView.gameObject;
        m_ScrollView.OnBeginDrag(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; ++i)
        {
            FunctionNode node = raycastResults[i].gameObject.GetComponentInParent<FunctionNode>();
            if (node && !node.IsTemplate)
            {
                if (!m_SelectedNodes.Contains(node))
                {
                    node.SetMessageSelected(true);
                    m_SelectedNodes.Add(node);
                }
                else
                {
                    node.SetMessageSelected(false);
                    m_SelectedNodes.Remove(node);
                }
                break;
            }
        }

        if (m_SelectedNodes.Count == 0)
        {
            m_RecordVoice.gameObject.SetActive(false);
            m_TextInputPanel.SetActive(false);
            m_ChoicePanel.gameObject.SetActive(false);
        }
        else if (!m_RecordVoice.gameObject.activeSelf &&
                 !m_TextInputPanel.activeSelf)
        {
            m_ChoicePanel.gameObject.SetActive(true);
        }
    }

    public MessageTag ActiveTag
    {
        get { return m_LastClickTag; }
    }

    internal void SetActiveTag(MessageTag tag)
    {
        m_LastClickTag = tag;
    }

    public void ReleaseActiveTag()
    {
        if (m_LastClickTag)
        {
            Assert.IsTrue(m_LastClickTag.IsOpen, "releasing a closed a message is unexpected");
            m_LastClickTag.ClickTag();
        }
        m_LastClickTag = null;
    }

    public void ClearSelectedNodes()
    {
        foreach (var node in m_SelectedNodes)
        {
            node.SetMessageSelected(false);
        }
        m_SelectedNodes.Clear();
        m_ChoicePanel.gameObject.SetActive(false);
    }

    public void ClickSaveText()
    {
        if (0 == m_SelectedNodes.Count)
        {
            return;
        }
        LeaveMessage msg = new LeaveMessage();
        msg.m_UserID = UserManager.Instance.UserId;
        msg.m_NickName = UserManager.Instance.Nickname;
        msg.m_Type = LeaveMessageType.Text;
        msg.TextLeaveMessage = m_Input.text;
        SaveLeaveMessage(msg);
    }

    public void SaveVoice(string voiceName)
    {
        LeaveMessage msg = new LeaveMessage();
        msg.m_UserID = UserManager.Instance.UserId;
        msg.m_NickName = UserManager.Instance.Nickname;
        msg.m_Type = LeaveMessageType.Voice;
        msg.TextLeaveMessage = voiceName;
        SaveLeaveMessage(msg);
    }

    void SaveLeaveMessage(LeaveMessage msg)
    {
        m_SelectedNodes.Sort((lhs, rhs) => rhs.transform.localPosition.y.CompareTo(lhs.transform.localPosition.y));

        string key = LeaveMessageList.GenerateKey(m_SelectedNodes.Select(x => x.NodeIndex));
        DeselectNodes();
        ClearPickList();

        var cmd = new AddLeaveMessageCommand(this, key, msg);
        m_Workspace.UndoManager.AddUndo(cmd);
    }

    void DeselectNodes()
    {
        for (int i = 0; i < m_SelectedNodes.Count; ++i)
        {
            m_SelectedNodes[i].SetMessageSelected(false);
        }
    }

    void ClearPickList()
    {
        m_Input.text = "";
        m_SelectedNodes.Clear();
        m_ChoicePanel.gameObject.SetActive(false);
        m_TextInputPanel.SetActive(false);
    }

    public void LoadMessages(byte[] data)
    {
        ClearMessageTag();
        //UpdateMessageCount(false);

        m_MessageDataSource.loadMessages(data);
        foreach (var msgList in m_MessageDataSource.messages)
        {
            AssociateMessageList(msgList);
            UpdateMessageCount(msgList, true);
        }
        Layout();
    }

    void UpdateMessageCount(LeaveMessageList msgList, bool add)
    {
        foreach (var nodeIndex in msgList.NodeIndices)
        {
            var node = CodePanel.GetNode(nodeIndex);
            if (node)
            {
                if (add)
                {
                    node.AddLeaveMessageCount();
                }
                else
                {
                    node.SubLeaveMessageCount();
                }
            }
        }
    }

    public void AddMessageList(LeaveMessageList msgList)
    {
        if (msgList.LeaveMessages.Count == 0)
        {
            return;
        }

        m_NeedLayout = true;
        UpdateMessageCount(msgList, true);
        var isNew = m_MessageDataSource.addMessage(msgList);
        if (isNew)
        {
            AssociateMessageList(msgList);
        }

        m_EventManager.SendEvent(EventId.UpdateLeaveMessage, msgList.Key);
    }

    public void DeleteMessageList(string key)
    {
        var msgList = m_MessageDataSource.getMessage(key);
        if (msgList != null)
        {
            m_NeedLayout = true;

            UpdateMessageCount(msgList, false);
            m_MessageDataSource.deleteMessage(msgList.Key);
            m_EventManager.SendEvent(EventId.UpdateLeaveMessage, msgList.Key);
        }
    }

    public void DeleteMessages(string key, IEnumerable<LeaveMessage> msgs)
    {
        var msgList = m_MessageDataSource.getMessage(key);
        if (msgList != null)
        {
            foreach (var msg in msgs)
            {
                msgList.RemoveMessage(msg);
            }

            if (msgList.LeaveMessages.Count == 0)
            {
                UpdateMessageCount(msgList, false);
                m_MessageDataSource.deleteMessage(key);

                m_NeedLayout = true;
            }

            m_EventManager.SendEvent(EventId.UpdateLeaveMessage, key);
        }
    }

    public void AddMessage(string key, LeaveMessage msg)
    {
        InsertMessage(key, msg, -1);
    }

    public void InsertMessage(string key, LeaveMessage msg, int index)
    {
        if (msg == null)
        {
            throw new ArgumentNullException("message");
        }

        var isNew = false;
        var msgList = m_MessageDataSource.getMessage(key);
        if (msgList == null)
        {
            if (index != 0 && index != -1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            isNew = true;
            msgList = new LeaveMessageList();
            msgList.Key = key;

            AssociateMessageList(msgList);

            m_MessageDataSource.addMessage(msgList);
        }

        if (index == -1)
        {
            msgList.LeaveMessages.Add(msg);
        }
        else
        {
            msgList.LeaveMessages.Insert(index, msg);
        }

        if (isNew)
        {
            UpdateMessageCount(msgList, true);
        }

        m_EventManager.SendEvent(EventId.UpdateLeaveMessage, key);
    }

    public void DeleteMessage(string key, int index)
    {
        var msgList = m_MessageDataSource.getMessage(key);
        if (msgList == null)
        {
            throw new ArgumentException("key");
        }

        msgList.LeaveMessages.RemoveAt(index);
        if (msgList.LeaveMessages.Count == 0)
        {
            m_MessageDataSource.deleteMessage(key);
            UpdateMessageCount(msgList, false);

            m_EventManager.SendEvent(EventId.UpdateLeaveMessage, key);
        }
    }

    void AssociateMessageList(LeaveMessageList msgList)
    {
        int tMainIndex = msgList.GetNodeAt(0);
        var node = CodePanel.GetNode(tMainIndex);
        if (node)
        {
            AssociateNodeToMessageTag(node, msgList.Key);
        }
    }

    void AssociateNodeToMessageTag(FunctionNode node, string key)
    {
        MessageTag tTagGroup;
        if (m_TagGroup.TryGetValue(node.NodeIndex, out tTagGroup))
        {
            tTagGroup.AddMessageTagBtn(key);
        }
        else
        {
            var newTag = GetTag();
            newTag.AddMessageTagBtn(key, node);

            Vector3 tNewPos = newTag.transform.position;
            tNewPos.y = node.transform.position.y;
            newTag.transform.position = tNewPos;

            m_TagGroup.Add(node.NodeIndex, newTag);

            m_NeedLayout = true;
        }
    }

    MessageTag GetTag()
    {
        GameObject go = Instantiate(m_TagTemplate, m_MessageContainer);
        go.SetActive(true);
        go.name = (m_TagGroup.Count + 1).ToString();
        return go.GetComponent<MessageTag>();
    }

    public void SetActive(bool show)
    {
        if (m_OnActivating != null)
        {
            m_OnActivating.Invoke(show);
        }

        ClearPickList();

        if (show)
        {
            ShowMessageFlag();
        }
        else
        {
            ReleaseActiveTag();
            HideMessageFlag();
        }

        m_MonitorBtn.SetActive(!show);
        m_MaskImage.SetActive(show);
        m_LeftMask.SetActive(show);
        if (!show)
        {
            m_MessageListView.gameObject.SetActive(false);
        }

        IsActive = show;
        m_InputContext.enabled = show;

        if (m_OnActivated != null)
        {
            m_OnActivated.Invoke(show);
        }
    }

    public bool IsActive
    {
        get;
        private set;
    }

    internal void ShowMessageFlag()
    {
        foreach (var node in CodePanel.Nodes)
        {
            node.EnableMessageStatus(true);
        }
    }

    void HideMessageFlag()
    {
        foreach (var node in CodePanel.Nodes)
        {
            node.ClearMessageStatus();
        }
    }

    internal void SetMessageOpened(string key, bool selected)
    {
        List<int> tNodeIndex = LeaveMessageList.ParseKey(key);
        for (int i = 0; i < tNodeIndex.Count; ++i)
        {
            var node = CodePanel.GetNode(tNodeIndex[i]);
            if (node)
            {
                node.SetMessageOpened(selected);
            }
        }
    }

    public bool IsEditMode()
    {
        return m_MaskImage.activeSelf;
    }

    public void ClickChoiceText()
    {
        m_ChoicePanel.gameObject.SetActive(false);
        m_TextInputPanel.SetActive(true);
    }

    public void ClickChoiceVoice()
    {
        m_ChoicePanel.gameObject.SetActive(false);
        m_RecordVoice.SetActive(true);
        m_RecordVoice.transform.SetAsLastSibling();
    }

    public void ClickCancelText()
    {
        m_TextInputPanel.SetActive(false);
        m_ChoicePanel.gameObject.SetActive(true);
    }

    public void ClickCancelVoice()
    {
        m_ChoicePanel.gameObject.SetActive(true);
        m_RecordVoice.SetActive(false);
    }

    public DeleteMessagesResult DeleteNodeMessages(int nodeIndex)
    {
        var deleteResult = new DeleteMessagesResult();

        var deletedKeys = new List<string>();
        var changedMsgList = new List<LeaveMessageList>();
        var changeEvents = new List<MessageKeyChangedEvent>();

        var targetNode = CodePanel.GetNode(nodeIndex);

        foreach (var msgList in m_MessageDataSource.messages)
        {
            // if the deleted node is the top-most node,
            // we need to delete the whole message
            if (nodeIndex == msgList.GetNodeAt(0))
            {
                deletedKeys.Add(msgList.Key);
                foreach (var id in msgList.NodeIndices)
                {
                    var node = CodePanel.GetNode(id);
                    if (node)
                    {
                        node.SubLeaveMessageCount();
                    }
                }

                deleteResult.deletedMsgLists.Add(msgList);
            }
            else if (msgList.ContainsNode(nodeIndex))
            {
                if (targetNode)
                {
                    targetNode.SubLeaveMessageCount();
                }

                var oldMsgList = new LeaveMessageList(msgList);

                var changeEvent = new MessageKeyChangedEvent();
                changeEvent.m_OldKey = msgList.Key;
                deletedKeys.Add(msgList.Key);

                msgList.RemoveNode(nodeIndex);
                changeEvent.m_NewKey = msgList.Key;

                changedMsgList.Add(msgList);
                changeEvents.Add(changeEvent);

                deleteResult.changedMessages.Add(new MessageKeyChangedData {
                    oldMsgList = oldMsgList,
                    newKey = msgList.Key
                });
            }
        }

        for (int i = 0; i < deletedKeys.Count; ++i)
        {
            m_MessageDataSource.deleteMessage(deletedKeys[i]);
        }
        for (int i = 0; i < changedMsgList.Count; ++i)
        {
            m_MessageDataSource.addMessage(changedMsgList[i]);
        }

        if (0 != changeEvents.Count)
        {
            m_EventManager.SendEvent(EventId.ChangeLeaveMessageKey, changeEvents);
        }

        RemoveMessageTag(nodeIndex);

        return deleteResult;
    }

    void RemoveMessageTag(int nodeIndex)
    {
        MessageTag tCurTag = null;
        if (m_TagGroup.TryGetValue(nodeIndex, out tCurTag))
        {
            Destroy(tCurTag.gameObject);
            m_TagGroup.Remove(nodeIndex);
        }

        m_NeedLayout = true;
    }

    internal void RemoveEmptyMessageTag(MessageTag tag)
    {
        if (tag == null)
        {
            throw new ArgumentNullException("tag");
        }
        if (tag.HasSelfMessages)
        {
            throw new InvalidOperationException();
        }
        RemoveMessageTag(tag.HeadNodeIndex);
    }

    public void Layout()
    {
        m_NeedLayout = false;

        var tags = new List<MessageTag>();
        foreach (var item in m_TagGroup)
        {
            if (item.Value.gameObject.activeSelf) {
                item.Value.ClearMerge();
                tags.Add(item.Value);
            }
        }

        // sort tags in top-down order
        tags.Sort((lhs, rhs) => rhs.transform.localPosition.y.CompareTo(lhs.transform.localPosition.y));

        // merge adjacent tags
        while (tags.Count >= 2)
        {
            // 0th tag is above the 1st tag
            float distance = tags[0].transform.localPosition.y - tags[1].transform.localPosition.y;
            if (distance <= m_MergeDistance)
            {
                tags[1].SetActive(false);
                tags[0].MergeWith(tags[1]);

                tags.RemoveAt(1);
            }
            else
            {
                tags.RemoveAt(0);
            }
        }
    }

    public MessageTag GetTagByID(int id)
    {
        MessageTag tCurTag = null;
        m_TagGroup.TryGetValue(id, out tCurTag);
        return tCurTag;
    }

    public void NodePickUpEventCallBack(object param)
    {
        foreach (var item in m_TagGroup)
        {
            item.Value.SetActive(true);
        }
    }

    public void SetLayoutDirty()
    {
        m_NeedLayout = true;
    }

    public void ClearMessageTag()
    {
        foreach (var item in m_TagGroup)
        {
            Destroy(item.Value.gameObject);
        }

        m_TagGroup.Clear();
    }
}
