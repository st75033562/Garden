using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using UnityEngine;

public class NetworkErrorSimulator : MonoBehaviour, IHttpClientFactory
{
    private class FakeHttpClient : IHttpClient
    {
        public int ID
        {
            get;
            set;
        }

        public void Abort() { }

        public Func<ResponseData> responseFactory;

        public void Get(string url, object parameter, Action<ResponseData> action, Dictionary<string, string> dic)
        {
            DoResponse(parameter, action);
        }

        public void Post(string url, byte[] bytes, object parameter, Action<ResponseData> action, Dictionary<string, string> dic)
        {
            DoResponse(parameter, action);
        }

        private void DoResponse(object parameter, Action<ResponseData> action)
        {
            var data = responseFactory();
            data.parameter = parameter;
            action(data);
        }

        public bool ProgressChanged
        {
            get;
            set;
        }

        public float Progress
        {
            get { return -1.0f; }
        }
    }

    private class FakeSocket : ISocket
    {
        private readonly AsyncSocket m_socket = new AsyncSocket();
        private readonly List<SocketResponse> m_responses = new List<SocketResponse>();

        public bool timeout;

        public bool connected
        {
            get
            {
                return m_socket.connected;
            }
        }

        public void clearAndClose()
        {
            m_socket.clearAndClose();
        }

        public void connect(string host, int port)
        {
            m_socket.connect(host, port);
        }

        public void getResponses(List<SocketResponse> packets)
        {
            m_socket.getResponses(m_responses);
            if (timeout)
            {
                m_responses.RemoveAll(x =>
                {
                    if (x.type == SocketResponseType.Data)
                    {
                        var cmd = CMD.Parser.ParseFrom(x.data);
                        if (cmd.Id == Command_ID.CmdEmpty)
                        {
                            return true;
                        }
                    }
                    return false;
                });
            }
            packets.AddRange(m_responses);
            m_responses.Clear();
        }

        public void send(byte[] data)
        {
            m_socket.send(data);
        }

        public int connectionTimeout
        {
            get;
            set;
        }
    }


    private bool m_simUnauthorization;
    private bool m_simTimeout;
    private FakeSocket m_fakeSocket;

    public void Initialize(SocketManager socketManager, WebRequestManager webRequestManager)
    {
        socketManager.initialize(() =>
        {
            m_fakeSocket = new FakeSocket();
            return m_fakeSocket;
        });
        webRequestManager.httpClientFactory = this;
    }

    void OnGUI()
    {
        int fontSize = (int)(16 * Mathf.Min(Screen.width / 640.0f, Screen.height / 480.0f));
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.toggle.fontSize = fontSize;
        GUILayout.BeginVertical();
        GUI.color = Color.red;
        m_simUnauthorization = GUILayout.Toggle(m_simUnauthorization, "Simulate Unauthorization");
        m_simTimeout = GUILayout.Toggle(m_simTimeout, "Simulate Timeout");
        if (m_fakeSocket != null)
        {
            m_fakeSocket.timeout = m_simTimeout;
        }
        GUILayout.EndVertical();
    }

    IHttpClient IHttpClientFactory.Create()
    {
        if (m_simUnauthorization)
        {
            var client = new FakeHttpClient();
            client.responseFactory = () =>
            {
                m_simUnauthorization = false;

                var response = new ResponseData();
                response.errorCode = HttpStatusCode.Unauthorized;
                response.error = response.errorCode.ToString();
                return response;
            };
            return client;
        }

        return new HttpClient();
    }
}
