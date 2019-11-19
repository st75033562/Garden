using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public static class FileUtils
{
    private static readonly char[] InvalidFileNameChars;

    static FileUtils()
    {
        if (Application.isMobilePlatform)
        {
            appTempPath = Path.Combine(Application.persistentDataPath, ".temp");
        }
        else
        {
            appTempPath = Path.Combine(Path.GetTempPath(), ApplicationUtils.identifier);
        }

        // 0x0-0x20, 0x7f are control characters
        var invalidChars = Enumerable.Range(0, 0x20).Select(x => (char)x).ToList();
        invalidChars.Add((char)0x7f);
        invalidChars.AddRange("\"<>|:*?\\/".ToCharArray());
        InvalidFileNameChars = invalidChars.ToArray();
    }

    // create the parent directory from the file path
    public static void createParentDirectory(string path)
    {
        createDirIfNotExist(Path.GetDirectoryName(path));
    }

    public static void createDirIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string appTempPath
    {
        get;
        private set;
    }

    public static void createAppTempPath()
    {
        if (!Directory.Exists(appTempPath))
        {
            Directory.CreateDirectory(appTempPath);
        }
    }

    public static void cleanupAppTempFiles()
    {
        try
        {
            if (Directory.Exists(appTempPath))
            {
                Directory.Delete(appTempPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        createAppTempPath();
    }

    public static string getTempFilename(string ext = "")
    {
        return Path.Combine(appTempPath, Guid.NewGuid().ToString("N") + ext);
    }

    public static string ensureExtension(string filename, string ext)
    {
        if (Path.GetExtension(filename) != ext)
        {
            return filename + ext;
        }
        return filename;
    }

    public static string normalizeSlash(string path)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return path.Replace("/", "\\");
#else
        return path.Replace("\\", "/");
#endif
    }

    /// <summary>
    /// move a file to the target path
    /// </summary>
    /// <param name="path">file path</param>
    /// <param name="target">file path or a directory path</param>
    public static void moveFile(string path, string target)
    {
        if (Directory.Exists(target))
        {
            target = Path.Combine(target, Path.GetFileName(path));
        }
        File.Move(path, target);
    }

    public static bool safeDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public static string ensureSlashIfNonEmpty(string path)
    {
        if (path == string.Empty) { return path; }
        return path.EndsWith("/") ? path : path + "/";
    }

    /// <summary>
    /// remove excessive files from the given directory.
    /// </summary>
    /// <param name="sorter">sort files, the first maxNum files will be preserved. 
    /// the default sorter is to sort file name in descending order
    /// </param>
    public static void ensureMaximumFiles(string dirPath, int maxNum, Comparison<FileInfo> sorter = null)
    {
        if (maxNum <= 0)
        {
            return;
        }

        var files = Directory.GetFiles(dirPath).Select(x => new FileInfo(x)).ToArray();
        if (files.Length > maxNum)
        {
            if (sorter == null)
            {
                sorter = (lhs, rhs) => rhs.Name.CompareTo(lhs.Name);
            }
            Array.Sort(files, sorter);
            for (int i = maxNum; i < files.Length; ++i)
            {
                File.Delete(files[i].FullName);
            }
        }
    }

    public static bool fileNameContainsInvalidChars(string name)
    {
        return name.IndexOfAny(InvalidFileNameChars) != -1;
    }

    public static bool isReservedFileName(string name)
    {
        // see https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
        return Regex.IsMatch(name, @"^(?:\.|\.\.|CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])$", RegexOptions.IgnoreCase);
    }

    // combine with forward slash
    public static string combine(string a, string b)
    {
        if (a == null)
        {
            throw new ArgumentNullException("a");
        }
        if (b == null)
        {
            throw new ArgumentNullException("b");
        }

        if (a == "")
        {
            return b;
        }

        if (a.EndsWith("/"))
        {
            if (b.StartsWith("/"))
            {
                return a + b.Substring(1);
            }
            else
            {
                return a + b;
            }
        }
        else
        {
            if (b.StartsWith("/"))
            {
                return a + b;
            }
            else
            {
                return a + "/" + b;
            }
        }
    }

    /// <summary>
    /// return the directory name of the path. if path is empty, return empty string.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string getDirectoryName(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return path == "" ? "" : Path.GetDirectoryName(path);
    }

    public static bool isParentPath(string path, string subPath)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }
        if (string.IsNullOrEmpty(subPath))
        {
            throw new ArgumentException("subPath");
        }

        path = GetPathWithoutTrailingSlash(path);
        subPath = GetPathWithoutTrailingSlash(subPath);

        if (subPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
        {
            if (path.Length == subPath.Length || subPath[path.Length] == '/')
            {
                return true;
            }
        }

        return false;
    }

    private static string GetPathWithoutTrailingSlash(string path)
    {
        if (path.Length > 0 && path[path.Length - 1] == '/')
        {
            return path.Substring(0, path.Length - 1);
        }
        return path;
    }

    public static string RemovePy(string path)
    {
        if(path.ToLower().EndsWith(".py")) {
            path = path.Substring(0, path.Length - 3);
        }
        return path;
    }

    public static void RecursionDirToFile(string dir, List<FileInfo> list) {
        DirectoryInfo d = new DirectoryInfo(dir);
        list.AddRange(d.GetFiles());
        DirectoryInfo[] directs = d.GetDirectories();//文件夹
        foreach(DirectoryInfo dd in directs) {
            RecursionDirToFile(dd.FullName, list);
        }
    }
}
