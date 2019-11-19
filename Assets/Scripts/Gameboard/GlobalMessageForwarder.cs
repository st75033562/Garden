using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Gameboard
{
    public class GlobalMessageForwarder : IMessageListener
    {
        private struct Receiver
        {
            public int id;
            public MessageManager manager;
        }

        private readonly List<Receiver> m_receivers = new List<Receiver>();

        public GlobalMessageForwarder(string prefix)
        {
            this.prefix = prefix;
        }

        public string prefix { get; private set; }

        public void AddReceiver(int id, MessageManager manager)
        {
            m_receivers.Add(new Receiver {
                id = id,
                manager = manager
            });
        }

        public void RemoveReceiver(MessageManager manager)
        {
            int index = m_receivers.FindIndex(x => x.manager == manager);
            if (index != -1)
            {
                m_receivers.RemoveAt(index);
            }
        }

        public void RemoveReceivers()
        {
            m_receivers.Clear();
        }

        public void onMessage(Message msg)
        {
            if (msg.scope == NameScope.Global)
            {
                if (msg.targetRobotIndices.Count > 0)
                {
                    foreach (var reciverId in msg.targetRobotIndices)
                    {
                        foreach (var receiver in m_receivers)
                        {
                            if (receiver.id == reciverId)
                            {
                                receiver.manager.broadcast(prefix + msg.name);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var receiver in m_receivers)
                    {
                        receiver.manager.broadcast(prefix + msg.name);
                    }
                }
            }
        }
    }
}
