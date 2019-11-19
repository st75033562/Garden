using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

public class Md5  {

    public static string CreateMD5Hash(byte[] input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(input);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    public static string HashString(string input)
    {
        return CreateMD5Hash(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>
    /// compute the md5 hash of the file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string HashFile(string file)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.Open(file, FileMode.Open))
        {
            return Utils.ToHex(md5.ComputeHash(stream));
        }
    }

    public static string Hash(byte[] input, int offset, int length)
    {
        using (var md5 = MD5.Create())
        using (var stream = new MemoryStream(input, offset, length))
        {
            return Utils.ToHex(md5.ComputeHash(stream));
        }
    }

    public static string Hash(byte[] input)
    {
        return Hash(input, 0, input.Length);
    }
}
