using Networking;
using System.Reflection;
using System.Linq;
using System;
using Gameboard.Script;
using Google.Protobuf;

namespace Gameboard
{
    abstract class RequestHandler
    {
        public abstract void Handle(ClientConnection conn, IMessage request);
    }

    abstract class RequestHandler<T> : RequestHandler where T : IMessage<T>
    {
        public override void Handle(ClientConnection conn, IMessage request)
        {
            Handle(conn, (T)request);
        }

        public abstract void Handle(ClientConnection conn, T request);
    }
}
