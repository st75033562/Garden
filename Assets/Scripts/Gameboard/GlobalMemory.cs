using System.Collections.Generic;

namespace Gameboard
{
    public class GlobalMemory
    {
        private readonly string m_globalPrefix;
        private readonly VariableManager m_source;
        private readonly List<VariableManager> m_clients = new List<VariableManager>();
        private bool m_isPropagatingChanges; // for avoiding recursion

        public GlobalMemory(string globalPrefix, VariableManager server)
        {
            m_globalPrefix = globalPrefix;
            m_source = server;
            m_source.onVariableChanged.AddListener(OnServerVariableChanged);
        }

        private void OnServerVariableChanged(BaseVariable variable)
        {
            if (variable.scope == NameScope.Global && !m_isPropagatingChanges)
            {
                m_isPropagatingChanges = true;
                foreach (var client in m_clients)
                {
                    var clientVar = client.get(m_globalPrefix + variable.name);
                    if (clientVar != null)
                    {
                        clientVar.readFrom(variable);
                    }
                }
                m_isPropagatingChanges = false;
            }
        }

        private void OnClientVariableChanged(BaseVariable variable)
        {
            if (!m_isPropagatingChanges &&
                variable.scope == NameScope.Global &&
                variable.name.StartsWith(m_globalPrefix))
            {
                m_isPropagatingChanges = true;
                var localName = variable.name.Substring(m_globalPrefix.Length);
                var serverVar = m_source.get(localName);
                if (serverVar != null)
                {
                    serverVar.readFrom(variable);
                }

                foreach (var client in m_clients)
                {
                    var clientVar = client.get(variable.name);
                    if (clientVar != variable)
                    {
                        clientVar.readFrom(variable);
                    }
                }
                m_isPropagatingChanges = false;
            }
        }

        public void AddClient(VariableManager client)
        {
            m_clients.Add(client);
            client.onVariableChanged.AddListener(OnClientVariableChanged);
        }

        public void RemoveClient(VariableManager client)
        {
            m_clients.Remove(client);
            client.onVariableChanged.RemoveListener(OnClientVariableChanged);
        }
    }
}
