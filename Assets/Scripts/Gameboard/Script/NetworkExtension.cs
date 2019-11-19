using Networking;
using Google.Protobuf;
using Gameboard.Script;

namespace Gameboard
{
    public static class NetworkExtension
    {
        public static void Send(this ClientConnection conn, Script.CommandId cmdId, IMessage msg)
        {
            conn.Send((int)cmdId, msg);
        }

        public static void RegisterHandler(this TcpServer server, CommandId cmdId, TcpServer.RequestHandler handler)
        {
            server.RegisterHandler((int)cmdId, handler);
        }
    }
}
