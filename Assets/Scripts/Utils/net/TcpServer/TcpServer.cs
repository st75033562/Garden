using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using UnityEngine;

namespace Networking
{
    public interface IRequestParser
    {
        IMessage Parse(int cmdId, ArraySegment<byte> data);
    }

    public partial class TcpServer : MonoBehaviour
    {
        public delegate void RequestHandler(ClientConnection c, IMessage request);

        public enum State
        {
            Stopped,
            Started,
            Stopping,
        }

        public event Action<ClientConnection> OnConnectionEstablished;
        public event Action<ClientConnection> OnConnectionClosed;

        // called when the server is stopped
        public event Action OnStopped;

        [SerializeField]
        private string m_ipAddress;

        [SerializeField]
        private int m_port;

        private TcpListener m_listener;
        private readonly List<ClientConnection> m_connections = new List<ClientConnection>();
        private readonly List<SocketResponse> m_socketRequests = new List<SocketResponse>();

        private delegate void RequestHandlerWrapper(ClientConnection conn, byte[] data, int offset);
        private readonly Dictionary<int, RequestHandler> m_requestHandlers = new Dictionary<int, RequestHandler>();
        private int m_nextConnectionId;

        void Awake()
        {
            CurrentState = State.Stopped;
        }

        void OnDestroy()
        {
            Stop();
        }

        public string Address
        {
            get { return m_ipAddress; }
            set
            {
                if (CurrentState != State.Stopped)
                {
                    throw new InvalidOperationException();
                }
                m_ipAddress = value;
            }
        }

        /// <summary>
        /// the listener port, maybe 0 if you want OS to select a port.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set
            {
                if (CurrentState != State.Stopped)
                {
                    throw new InvalidOperationException();
                }
                m_port = value;
            }
        }

        /// <summary>
        /// request parser, must be set before running the server
        /// </summary>
        public IRequestParser RequestParser
        {
            get;
            set;
        }

        public State CurrentState
        {
            get;
            private set;
        }

        public void Run()
        {
            if (CurrentState != State.Stopped)
            {
                throw new InvalidOperationException("Already running");
            }

            if (RequestParser == null)
            {
                throw new InvalidOperationException("RequestParser not set");
            }

            var address = IPAddress.Any;
            if (!string.IsNullOrEmpty(m_ipAddress))
            {
                address = IPAddress.Parse(m_ipAddress);
            }

            CurrentState = State.Started;
            m_listener = new TcpListener(address, Port);
            m_listener.Start();
            if (m_port == 0)
            {
                m_port = (m_listener.LocalEndpoint as IPEndPoint).Port;
            }

            m_listener.BeginAcceptSocket(OnAcceptedSocket, null);
        }

        public void Stop()
        {
            if (CurrentState == State.Started)
            {
                CurrentState = State.Stopping;
                // handlers are not cleared
                m_listener.Stop();
                m_nextConnectionId = 0;
                CloseConnections();
            }
        }

        private void OnAcceptedSocket(IAsyncResult res)
        {
            try
            {
                // when Stop was called, this can be null
                if (m_listener.Server == null)
                {
                    return;
                }

                var socket = m_listener.EndAcceptSocket(res);
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                CallbackQueue.instance.Enqueue(() => {
                    if (CurrentState == State.Started)
                    {
                        var conn = new ClientConnection(this, m_nextConnectionId++, new AsyncSocket(socket));
                        m_connections.Add(conn);

                        Debug.LogFormat("new connection from: {0}, id: {1}", socket.RemoteEndPoint, conn.Id);
                        if (OnConnectionEstablished != null)
                        {
                            OnConnectionEstablished(conn);
                        }
                    }
                    else
                    {
                        Debug.Log("server stopped, closing incoming connection");
                        socket.Close();
                    }
                });

                m_listener.BeginAcceptSocket(OnAcceptedSocket, null);
            }
            catch (ObjectDisposedException)
            {
                // in case listening was closed
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void RegisterHandler(int cmd, RequestHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException();
            }

            m_requestHandlers.Add(cmd, handler);
        }

        public ClientConnection FindConnection(int id)
        {
            return m_connections.Find(x => x.Id == id);
        }

        public int ConnectionCount
        {
            get { return m_connections.Count; }
        }

        public IEnumerable<ClientConnection> Connections
        {
            get { return m_connections; }
        }

        internal void OnClosingConnection(ClientConnection conn)
        {
            m_connections.Remove(conn);
            if (OnConnectionClosed != null)
            {
                OnConnectionClosed(conn);
            }
        }

        public void CloseConnections()
        {
            var connections = m_connections.ToArray();
            m_connections.Clear();
            foreach (var conn in connections)
            {
                conn.Close();
            }
        }

        private void Update()
        {
            for (int i = m_connections.Count - 1; i >= 0; --i)
            {
                var conn = m_connections[i];
                conn.Socket.getResponses(m_socketRequests);

                foreach (var request in m_socketRequests)
                {
                    if (request.type == SocketResponseType.Data)
                    {
                        DispatchMessage(conn, request.data);
                    }
                    else
                    {
                        m_connections.RemoveAt(i);
                        conn.Close();
                        break;
                    }
                }
                m_socketRequests.Clear();
            }

            if (CurrentState == State.Stopping && m_connections.Count == 0)
            {
                CurrentState = State.Stopped;
                if (OnStopped != null)
                {
                    OnStopped();
                }
            }
        }

        private void DispatchMessage(ClientConnection conn, byte[] data)
        {
            try
            {
                int cmdId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
                RequestHandler handler;
                if (m_requestHandlers.TryGetValue(cmdId, out handler))
                {
                    var requestData = new ArraySegment<byte>(data, sizeof(int), data.Length - sizeof(int));
                    var request = RequestParser.Parse(cmdId, requestData);
                    if (request != null)
                    {
                        handler(conn, request);
                    }
                    else
                    {
                        Debug.LogError("cannot parse request for " + cmdId);
                    }
                }
                else
                {
                    Debug.LogWarning("no handler found for request: " + cmdId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogException(e);
            }
        }

        private void OnApplicationQuit()
        {
            Stop();
        }
    }

}
