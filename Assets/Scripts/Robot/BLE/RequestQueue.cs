using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Robomation.BLE
{
    enum RequestType
    {
        Connection,
        Disconnection,
        DiscoverService,
        Subscribe,
        WriteMotoring,
    }

    class Request
    {
        public const float RequestTimeout = 7.0f;
        public const float WriteRequestTimeout = 1.0f;

        public string robotId;
        public RequestType type;
        public byte[] writeData = null; // used for write request

        public float timeout
        {
            get
            {
                if (type <= RequestType.Subscribe)
                {
                    return RequestTimeout;
                }
                else
                {
                    return WriteRequestTimeout;
                }
            }
        }
    }

    class RequestQueue
    {
        private readonly LinkedList<Request> m_requests = new LinkedList<Request>();

        private bool m_busy;
        private float m_timer;

        public void prepend(Request request)
        {
            m_requests.AddFirst(request);
        }

        public void append(Request request)
        {
            m_requests.AddLast(request);
        }

        public LinkedListNode<Request> find(string id, RequestType type)
        {
            for (var p = m_requests.First; p != null; p = p.Next)
            {
                if (p.Value.robotId == id &&
                    (p.Value.type & type) != 0)
                {
                    return p;
                }
            }
            return null;
        }

        public bool hasRequests(string id)
        {
            return m_requests.Contains(x => x.robotId == id);
        }

        public void remove(LinkedListNode<Request> node)
        {
            Assert.IsTrue(node.List == m_requests, "invalid node");

            if (node.Previous == null)
            {
                busy = false;
            }
            m_requests.Remove(node);

            Assert.IsTrue(count != 0 || !busy, "invalid busy state: remove");
        }

        public bool busy
        {
            get { return m_busy; }
            set
            {
                if (m_busy != value)
                {
                    if (value)
                    {
                        Assert.IsTrue(count > 0, "cannot set busy on an empty queue");
                        startTimer();
                    }
                    else
                    {
                        stopTimer();
                    }
                    m_busy = value;
                }
            }
        }

        public Request first
        {
            get
            {
                Assert.IsTrue(count > 0, "empty queue");

                return m_requests.First.Value;
            }
        }

        public int count
        {
            get { return m_requests.Count; }
        }

        public void reset()
        {
            m_requests.Clear();
            busy = false;
        }

        public bool updateTimer()
        {
            if (m_timer > 0)
            {
                m_timer -= Time.unscaledDeltaTime;
                if (m_timer <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void dequeueRequest()
        {
            m_requests.RemoveFirst();
            busy = false;
        }

        private void startTimer()
        {
            m_timer = first.timeout;
        }

        private void stopTimer()
        {
            m_timer = 0;
        }

        public void removeAllRequests(string identifier, bool skipBusy = false)
        {
            if (count > 0)
            {
                //s_logger.Log("remove all requests, {0}, {1}", name, identifier);
                //s_logger.Log(this);

                var p = m_requests.First;
                if (p.Value.robotId == identifier)
                {
                    if (skipBusy && busy)
                    {
                        p = p.Next;
                    }
                    else if (!skipBusy)
                    {
                        busy = false;
                    }
                }
                else
                {
                    p = p.Next;
                }

                if (p == null)
                {
                    return;
                }

                m_requests.RemoveAll(p, x => x.robotId == identifier); ;

                //s_logger.Log(this);

                Assert.IsTrue(count != 0 || !busy, "invalid busy state: removeAllRequests");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("busy: {0}\n", busy);
            sb.AppendFormat("timer: {0}\n", m_timer);
            foreach (var request in m_requests)
            {
                sb.AppendFormat("type: {0}, robot: {1}\n", request.type, request.robotId);
            }
            return sb.ToString();
        }
    }
}
