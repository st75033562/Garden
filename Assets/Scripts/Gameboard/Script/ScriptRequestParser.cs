using Google.Protobuf;
using Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Gameboard
{
    public class ScriptRequestParser : IRequestParser
    {
        private readonly Dictionary<int, MessageParser> m_parsers = new Dictionary<int, MessageParser>();

        public static readonly ScriptRequestParser instance = new ScriptRequestParser();

        private ScriptRequestParser()
        {
            var cmds = (int[])Enum.GetValues(typeof(Script.CommandId));
            var names = Enum.GetNames(typeof(Script.CommandId));
            for (int i = 0; i < names.Length; ++i)
            {
                var requestName = "Gameboard.Script." + names[i].Substring(3) + "Request";
                var type = Type.GetType(requestName);
                if (type != null)
                {
                    var prop = type.GetProperty("Parser", BindingFlags.Static | BindingFlags.Public);
                    m_parsers.Add(cmds[i], (MessageParser)prop.GetValue(null, null));
                }
            }
        }

        public IMessage Parse(int cmdId, ArraySegment<byte> data)
        {
            MessageParser parser;
            if (m_parsers.TryGetValue(cmdId, out parser))
            {
                return parser.ParseFrom(new CodedInputStream(data.Array, data.Offset, data.Count));
            }
            else
            {
                return null;
            }
        }
    }
}
