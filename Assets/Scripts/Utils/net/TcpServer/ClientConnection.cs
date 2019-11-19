using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Networking
{
    public class ClientConnection
    {
        private readonly AsyncSocket m_socket;
        private bool m_closed;

        internal ClientConnection(TcpServer svr, int id, AsyncSocket s)
        {
            m_socket = s;
            Server = svr;
            Id = id;
        }

        /// <summary>
        /// send the raw data to the client
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            if (m_closed)
            {
                Debug.LogError("already closed");
                return;
            }

            m_socket.send(data);
        }

        /// <summary>
        /// send protobuf message to the client
        /// </summary>
        public void Send(int cmdId, IMessage message)
        {
            const int HeaderSize = 2 * sizeof(int);
            int payloadSize = message.CalculateSize();
            var buffer = new byte[payloadSize + HeaderSize];
            BufferUtils.WriteNetworkOrder(buffer, 0, payloadSize);
            BufferUtils.WriteNetworkOrder(buffer, sizeof(int), cmdId);
            Array.Copy(message.ToByteArray(), 0, buffer, HeaderSize, payloadSize);
            Send(buffer);
        }

        public TcpServer Server
        {
            get;
            private set;
        }

        // unique id of the connection
        public int Id
        {
            get;
            private set;
        }

        // close the connection, no further send possible
        public void Close()
        {
            if (!m_closed)
            {
                Debug.Log("close connection " + Id);

                m_closed = true;
                // #TODO should not forcibly close the socket
                m_socket.clearAndClose();
                Server.OnClosingConnection(this);
            }
        }

        public bool Closed
        {
            get { return m_closed; }
        }

        internal AsyncSocket Socket
        {
            get { return m_socket; }
        }
    }

}
