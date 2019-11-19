using System;
using System.IO;
using UnityEngine;

public class VoiceRepository
{
    public const string DefaultRoot = ".voices";

    private readonly string m_root;

    public static readonly VoiceRepository instance = new VoiceRepository();

    public VoiceRepository(string root = DefaultRoot)
    {
        m_root = Application.persistentDataPath + "/" + root;
        if (!m_root.EndsWith("/"))
        {
            m_root += "/";
        }

        if (!Directory.Exists(m_root))
        {
            Directory.CreateDirectory(m_root);
        }
    }

    public bool save(string voiceName, byte[] data)
    {
        if (string.IsNullOrEmpty(voiceName))
        {
            throw new ArgumentException("empty voice name");
        }

        try
        {
            File.WriteAllBytes(m_root + voiceName, data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public byte[] load(string voiceName)
    {
        if (string.IsNullOrEmpty(voiceName))
        {
            throw new ArgumentException("empty voice name");
        }

        try
        {
            string path = m_root + voiceName;
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return null;
    }

    public void delete(string voiceName)
    {
        if (string.IsNullOrEmpty(voiceName))
        {
            throw new ArgumentException("empty voice name");
        }

        try
        {
            string path = m_root + voiceName;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
