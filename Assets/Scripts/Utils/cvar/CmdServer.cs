using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public delegate string CmdHandler(string[] args);

public class CmdServer
{
    public const int DefaultPort = 5666;

    private static readonly A8.Logger s_logger = new A8.Logger("CmdServer");
    private static readonly Dictionary<string, CmdHandler> s_commands = new Dictionary<string, CmdHandler>();
    private static Thread s_serverThread;
    private static bool s_stopped;
    private static HttpListener s_listener;
    private static EventWaitHandle s_processedEvent;

    public static void Start(int port = DefaultPort)
    {
        if (s_serverThread != null)
        {
            throw new InvalidOperationException();
        }

        if (s_listener == null)
        {
            s_listener = new HttpListener();
            s_listener.Prefixes.Add("http://*:" + port + "/");
            s_listener.Prefixes.Add("http://localhost:" + port + "/");
        }

        s_processedEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        s_stopped = false;
        s_serverThread = new Thread(ListenerMain);
        s_serverThread.Start(port);
    }

    public static void Shutdown()
    {
        if (s_serverThread != null)
        {
            s_stopped = true;
            s_processedEvent.Set();
            try
            {
                s_listener.Stop();
            }
            catch (SocketException e)
            {
                // this could happen when editor is running. it seems editor cannot clean up the socket properly.
                s_logger.LogException(e);
            }
            s_serverThread.Join();
            s_processedEvent.Close();
            s_serverThread = null;
        }
    }

    public static void Register(string cmdName, IVarCommand cmd)
    {
        if (cmd == null)
        {
            throw new ArgumentNullException("cmd");
        }
        s_commands.Add(cmdName, cmd.Execute);
    }

    public static void Register(string cmdName, CmdHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
        s_commands.Add(cmdName, handler);
    }
    
    public static void Unregister(string cmdName)
    {
        s_commands.Remove(cmdName);
    }

    private static void ListenerMain(object arg)
    {
        try
        {
            s_listener.Start();
            foreach (var prefix in s_listener.Prefixes)
            {
                s_logger.Log("listening at " + prefix);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }

        while (!s_stopped)
        {
            HttpListenerContext context = null;
            
            try
            {
                context = s_listener.GetContext();    
            }
            catch (Exception)
            {
                continue;
            }

            try
            {
                HandleRequest(context);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                context.Response.Close();
            }
        }
        s_listener.Stop();
        s_logger.Log("shutdown");
    }

    private static void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.Url == null)
        {
            return;
        }

        bool waitUntilProcessed = false;
        string responseData = string.Empty;
        string error = null;
        if (request.Url.LocalPath == "/shutdown")
        {
            s_stopped = true;
        }
        else if (request.Url.LocalPath == "/cmd")
        {
            try
            {
                using (var reader = new StreamReader(request.InputStream))
                {
                    var requestData = JsonMapper.ToObject(reader.ReadToEnd());
                    var args = requestData["args"];
                    CmdHandler handler;
                    if (s_commands.TryGetValue((string)args[0], out handler))
                    {
                        string[] cmdArgs = new string[args.Count - 1];
                        for (int i = 1; i < args.Count; ++i)
                        {
                            cmdArgs[i - 1] = (string)args[i];
                        }
                        waitUntilProcessed = true;
                        CallbackQueue.instance.Enqueue(() => {
                            try
                            {
                                responseData = handler(cmdArgs);
                            }
                            catch (Exception e)
                            {
                                error = e.Message;
                            }
                            s_processedEvent.Set();
                        });
                    }
                    else
                    {
                        error = "invalid command: " + args[0];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                error = e.Message;
            }
        }

        if (waitUntilProcessed)
        {
            s_processedEvent.WaitOne();
        }

        try
        {
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                var responseObj = new JsonData();
                responseObj["res"] = responseData ?? string.Empty;
                responseObj["err"] = error;
                string data = responseObj.ToJson();
                context.Response.ContentLength64 = data.Length;
                writer.Write(data);
            }
        }
        catch (Exception e)
        {
            s_logger.LogError("Failed to write response: " + e.Message);
        }
    }
}
