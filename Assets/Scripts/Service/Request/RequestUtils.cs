using System;
using System.Linq;

public static class RequestUtils
{
    public static string Base64Encode(string str)
    {
        byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
        string base64Name = Convert.ToBase64String(b);
        return base64Name.Replace('/', '-');
    }

    public static string EncodePath(string str)
    {
        return string.Join("/", str.Split('/').Select(x => Base64Encode(x)).ToArray());
    }
}
