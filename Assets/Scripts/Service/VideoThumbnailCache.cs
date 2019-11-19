using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

public class VideoThumbnailCache
{
    private const int ThumbnailWidth = 256;
    private const string DefaultThumnailName = "video_thumb2.jpg";

    private static string s_cachePath;

    public static void Init()
    {
        string cachePath = Application.persistentDataPath + "/.video_thumbs/";
        if (!Directory.Exists(cachePath))
        {
            Directory.CreateDirectory(cachePath);
        }
        s_cachePath = cachePath;


        if (Application.platform == RuntimePlatform.Android)
        {
            defaultThumbnailPath = s_cachePath + DefaultThumnailName;
        }
        else
        {
            defaultThumbnailPath = Application.streamingAssetsPath + "/" + DefaultThumnailName;
        }
    }

    public static IEnumerator PostInit()
    {
        // extract the default video thumbnail
        if (Application.platform == RuntimePlatform.Android)
        {
            if (File.Exists(defaultThumbnailPath))
            {
                yield break;
            }

            using (var www = new WWW(Application.streamingAssetsPath + "/" + DefaultThumnailName))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    File.WriteAllBytes(defaultThumbnailPath, www.bytes);
                    Debug.Log("extracted default video thumbnail");
                }
                else
                {
                    Debug.LogError("unable to extract default video thumbnail");
                }
            }
        }
    }

    public static string defaultThumbnailPath
    {
        get;
        private set;
    }

    /// <summary>
    /// get thumbnail for the given local video
    /// </summary>
    /// <param name="videoPath">path to a local video</param>
    /// <returns>null if error happens</returns>
    public static string GetThumbnailPath(string videoPath)
    {
        var path = GenerateThumbnailPath(videoPath);
        try
        {
            // TODO: remove hard coded time
            if (!File.Exists(path) && !ThumbnailUtils.Generate(videoPath, path, 1, ThumbnailWidth))
            {
                Debug.LogError("failed to generate thumnail for " + videoPath);
                return null;
            }
            return path;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    /// <summary>
    /// create a thumbnail for a local video
    /// </summary>
    /// <param name="videoPath">path to a local video</param>
    /// <returns>null if error occurs</returns>
    public static string CreateThumbnail(string videoPath)
    {
        // TODO: remove hard coded time
        var path = GenerateThumbnailPath(videoPath);
        if (ThumbnailUtils.Generate(videoPath, path, 1, ThumbnailWidth))
        {
            return path;
        }
        Debug.LogError("failed to generate thumnail for " + videoPath);
        return null;
    }

    private static string GenerateThumbnailPath(string videoPath)
    {
        string name = Md5.HashString(FileUtils.normalizeSlash(videoPath));
        return s_cachePath + name.Substring(0, 2) + "/" + name;
    }
}
