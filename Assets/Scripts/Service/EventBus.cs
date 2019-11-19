using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EventId
{
    UpdateAvatar,
    UpdateClassThumbnail,
    UpdateLeaveMessage,
    ChangeLeaveMessageKey,
    DeleteNode,
    PutDownNode,
    PickUpNode,
    PutDownNode_LateRefresh,
    NodeUnplugged, // inserted node was unplugged
    NodePluginChanged,

    KeyPressed,
    EventBrodcasted,
    GuideInput,

    LocalVideoShared,

    CompetitionCreated,
    CompetitionRemoved,
    CompetitionUpdated,
    CompetitionProblemUpdated,

    UserLoggedIn,
    GuideInvalidInput
}

public class EventBus : MonoBehaviour
{
    struct EventData
    {
        public int m_EventID;
        public object m_Param;
    }

    Dictionary<int, List<Action<object>>> m_EventTable = new Dictionary<int, List<Action<object>>>();
    Queue<EventData> m_Event = new Queue<EventData>();

    private bool m_sendingEvent;

    // the default global instance
    public static EventBus Default
    {
        get { return Singleton<EventBus>.instance; }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        while (0 != m_Event.Count)
        {
            EventData tCurEvent = m_Event.Dequeue();
            SendEvent(tCurEvent.m_EventID, tCurEvent.m_Param);
        }
    }

    public void AddListener(EventId eventId, Action<object> callback)
    {
        AddListener((int)eventId, callback);
    }

    public void AddListener(int eventId, Action<object> callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException("callback");
        }

        List<Action<object>> tFunction = null;
        if (m_EventTable.TryGetValue(eventId, out tFunction))
        {
            tFunction.Add(callback);
        }
        else
        {
            tFunction = new List<Action<object>>();
            tFunction.Add(callback);
            m_EventTable.Add(eventId, tFunction);
        }
    }

    public void RemoveListener(EventId eventId, Action<object> callback)
    {
        RemoveListener((int)eventId, callback);
    }

    public void RemoveListener(int eventId, Action<object> callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException("callback");
        }

        if (m_sendingEvent)
        {
            Debug.LogError("cannot remove event when there's an outstanding event");
            return;
        }

        List<Action<object>> tFunction;
        if (m_EventTable.TryGetValue(eventId, out tFunction))
        {
            tFunction.Remove(callback);
        }
    }

    public void AddEvent(EventId eventId, object param = null)
    {
        AddEvent((int)eventId, param);
    }

    public void AddEvent(int eventId, object param = null)
    {
        EventData tNew = new EventData();
        tNew.m_EventID = eventId;
        tNew.m_Param = param;
        m_Event.Enqueue(tNew);
    }

    public void SendEvent(EventId eventId, object param = null)
    {
        SendEvent((int)eventId, param);
    }

    /// <summary>
    /// send the event immediately
    /// </summary>
    public void SendEvent(int eventId, object param = null)
    {
        if (m_sendingEvent)
        {
            Debug.Log("cannot send event when there's an outstanding event");
            return;
        }

        m_sendingEvent = true;
        List<Action<object>> tFunction = null;
        if (m_EventTable.TryGetValue(eventId, out tFunction))
        {
            for (int i = 0; i < tFunction.Count; )
            {
                MonoBehaviour tCurObj = (MonoBehaviour)tFunction[i].Target;
                if (null != tCurObj)
                {
                    try
                    {
                        tFunction[i](param);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    ++i;
                }
                else
                {
                    tFunction.RemoveAt(i);
                }
            }
        }
        m_sendingEvent = false;
    }
}
