using System;
using System.Linq;

public class ContentRangeHeader
{
    public ContentRangeHeader(long length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length");
        }

        Init(null, null, length);
    }

    public ContentRangeHeader(long start, long end)
    {
        if (start < 0 || end < start)
        {
            throw new ArgumentOutOfRangeException();
        }

        Init(start, end, null);
    }

    public ContentRangeHeader(long start, long end, long length)
    {
        if (start < 0 || end < start || length < end + 1)
        {
            throw new ArgumentOutOfRangeException();
        }

        Init(start, end, length);
    }

    private void Init(long? start, long? end, long? length)
    {
        Unit = "bytes";
        Start = start;
        End = end;
        Length = length;
    }

    public string Unit { get; set; }

    public long? Start { get; private set; }

    public long? End { get; private set; }

    public long? Length { get; private set; }
    

    public static bool TryParse(string value, out ContentRangeHeader header)
    {
        header = null;

        var tokens = value.Split(' ');
        if (tokens.Length != 2)
        {
            return false;
        }

        var rangeSpecTokens = tokens[1].Split('/');
        if (rangeSpecTokens.Length != 2)
        {
            return false;
        }

        int start = -1, end = -1;
        if (rangeSpecTokens[0].Contains('-'))
        {
            var rangeTokens = rangeSpecTokens[0].Split('-');
            if (rangeTokens.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(rangeTokens[0], out start))
            {
                return false;
            }

            if (!int.TryParse(rangeTokens[1], out end))
            {
                return false;
            }
        }
        else if (rangeSpecTokens[0] != "*")
        {
            return false;
        }

        int length = -1;
        if (rangeSpecTokens[1] != "*")
        {
            if (!int.TryParse(rangeSpecTokens[1], out length))
            {
                return false;
            }
        }

        try
        {
            if (start != -1 && end != -1 && length != -1)
            {
                header = new ContentRangeHeader(start, end, length);
            }
            else if (start != -1 && end != -1)
            {
                header = new ContentRangeHeader(start, end);
            }
            else
            {
                header = new ContentRangeHeader(length);
            }

            header.Unit = tokens[0];
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static ContentRangeHeader Parse(string value)
    {
        ContentRangeHeader header;
        if (!TryParse(value, out header))
        {
            throw new FormatException();
        }
        return header;
    }
}