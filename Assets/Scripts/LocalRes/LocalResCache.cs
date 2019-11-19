using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LocalResCache {

    public static readonly LocalResCache instance = new LocalResCache();

    private string RootPath(string name)
    {
        return Application.persistentDataPath + "/" + UserManager.Instance.UserId + "/LocalResCahce/"+ name;
    }

    public bool SaveImage(string name, byte[] data)
    {
        string path = RootPath(name);
        if (File.Exists(path))
            return true;
        try
        {
            FileUtils.createParentDirectory(path);
            using (Stream fs = File.Create(path))
            {
                fs.Write(data, 0, data.Length);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public byte[] LoadImage(string name)
    {
        string path = RootPath(name);
        if (!File.Exists(path))
            return null;
        try
        {
            byte[] data = File.ReadAllBytes(path);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public bool Delete(string name)
    {
        return FileUtils.safeDelete(RootPath(name));
    }
}
