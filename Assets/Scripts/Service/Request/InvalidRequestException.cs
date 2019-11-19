using System;

public class InvalidRequestException : Exception
{
    public InvalidRequestException() { }

    public InvalidRequestException(string message)
        : base(message)
    { }
}
