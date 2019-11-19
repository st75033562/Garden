using Google.Protobuf;
using System;

public static class ProtobufUtils
{
    public static T Parse<T>(byte[] data)  where T : IMessage<T>, new()
    {
        return new MessageParser<T>(() => new T()).ParseFrom(data);
    }

    public static T Parse<T>(ByteString data)  where T : IMessage<T>, new()
    {
        return new MessageParser<T>(() => new T()).ParseFrom(data);
    }

    public static Guid ToGuid(ByteString bs)
    {
        return new Guid(bs.ToByteArray());
    }

    public static ByteString ToByteString(Guid guid)
    {
        return ByteString.CopyFrom(guid.ToByteArray());
    }
}
