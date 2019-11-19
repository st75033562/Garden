using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public static class Utils  {

	public static bool IsValidUrl(string url)
    {
        try
        {
            new Uri(url);
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    public static Vector3 ToVec3(Vector2 p, float z = 0.0f)
    {
        return new Vector3(p.x, p.y, z);
    }

    public static string UrlEncode(string url)
    {
        var uri = new Uri(url);
        if (uri.Query == "")
        {
            return url;
        }

        // remove `?'
        var queryDict = ParseQueryString(uri.Query.Substring(1));

        var sb = new StringBuilder();
        foreach (var kv in queryDict)
        {
            foreach (var v in kv.Value)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }
                sb.AppendFormat("{0}={1}", kv.Key, v);
            }
        }

        var uriBuilder = new UriBuilder(uri);
        uriBuilder.Query = sb.ToString();
        return uriBuilder.ToString();
    }

    public static Dictionary<string, string[]> ParseQueryString(string query, bool escape = true)
    {
        var dict = new Dictionary<string, string[]>();
        foreach (var kv in query.Split('&'))
        {
            var tokens = kv.Split('=');
            var key = tokens[0];
            string value = string.Empty;
            if (tokens.Length > 1)
            {
                value = escape ? WWW.EscapeURL(tokens[1]) : tokens[1];
            }

            string[] values;
            if (!dict.TryGetValue(key, out values))
            {
                values = new string[] { value };
                dict.Add(key, values);
            }
            else
            {
                var newValues = new string[values.Length + 1];
                Array.Copy(values, newValues, values.Length);
                newValues[newValues.Length - 1] = value;
                dict[key] = newValues;
            }
        }

        return dict;
    }
	
	    /// <summary>
    /// copy source to the target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="maxBytes">max bytes to copy, if <= 0, no limit</param>
    /// <param name="buf"></param>
    /// <returns>number of copied bytes</returns>
    public static int Copy(Stream source, Stream dest, int maxBytes = 0, byte[] buf = null)
    {
        if (buf == null)
        {
            buf = new byte[4096];
        }

        int remainingBytes = maxBytes;
        int bytesCopied = 0;
        while (maxBytes <= 0 || remainingBytes > 0)
        {
            int read = source.Read(buf, 0, Mathf.Min(buf.Length, maxBytes));
            if (read > 0)
            {
                dest.Write(buf, 0, read);
                remainingBytes -= read;
                bytesCopied += read;
            }
            else
            {
                break;
            }
        }
        return bytesCopied;
    }

    public static string ToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

#if UNITY_ANDROID
    public static AndroidJavaObject GetUnityActivity()
    {
        using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public static bool IsWifiConnected()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unity = GetUnityActivity())
        {
            using (var wifiManager = unity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                return wifiManager.Call<bool>("isWifiEnabled");
            }
        }
#else
        return false;
#endif
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }

    public static void Swap<T>(IList<T> list, int i, int j)
    {
        T tmp = list[i];
        list[i] = list[j];
        list[j] = tmp;
    }
    
    public static void Rotate<T>(IList<T> list, int newHeadIndex)
    {
        Reverse(list, 0, newHeadIndex);
        Reverse(list, newHeadIndex, list.Count);
        Reverse(list, 0, list.Count);
    }

    public static void Reverse<T>(IList<T> list, int start, int end)
    {
        while (start < end - 1)
        {
            Swap(list, start++, --end);
        }
    }

    public static Camera FindCamera(int layer)
    {
        foreach (var camera in Camera.allCameras)
        {
             if (((camera.cullingMask) & (1 << layer)) != 0)
             {
                 return camera;
             }
        }
        return null;
    }

    public static string QuoteArgument(string arg)
    {
        return "\"" + arg + "\"";
    }

    public static Process RunInTerminal(string command, out string tempScriptPath, Dictionary<string, string> env = null)
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        var proc = new Process();
        tempScriptPath = FileUtils.getTempFilename();

        string preambles = "#! /bin/sh\nclear\n";
        if (env != null)
        {
            // manually export the environment variables, don't known why adding
            // to `EnvironmentVariables' doesn't work
            foreach (var entry in env)
            {
                preambles += string.Format("export {0}={1}\n", entry.Key, entry.Value);
            }
        }
        File.WriteAllText(tempScriptPath, preambles + command);
        using (Process.Start("chmod", "+x " + tempScriptPath)) {};
        proc.StartInfo.FileName = "open";
        proc.StartInfo.Arguments = "-a Terminal " + tempScriptPath;
        proc.Start();
        return proc;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        var proc = new Process();
        tempScriptPath = FileUtils.getTempFilename(".bat");
        proc.StartInfo.FileName = tempScriptPath;
        string enviromentVars = string.Empty;
        if (env != null)
        {
            foreach (var kv in env)
            {
                enviromentVars += string.Format("@set {0}={1}\r\n", kv.Key, kv.Value);
            }
        }
        // batch file has to be OEM encoded
        File.WriteAllText(tempScriptPath, string.Format("{0}\r\n@call {1}\r\n@pause", enviromentVars, command), Encoding.Default);
        proc.Start();
        return proc;
#else
        throw new NotImplementedException();
#endif
    }

    public static string EllipsisChar(this string str, int showCount = 8) {
        string result = str;
        if(str.Length > showCount) {
            result = str.Substring(0, showCount) + "...";
        }
        return result;
    }


    public static SetComparisonResult<T> Compare<T>(
        IEnumerable<T> leftColl, IEnumerable<T> rightColl, Func<T, T, bool> comp, bool computeCommon = true)
    {
        var res = new SetComparisonResult<T>();
        var comparer = DelegatedEqualityComparer.Of(comp);
        var leftSet = new HashSet<T>(leftColl, comparer);
        var rightSet = new HashSet<T>(rightColl, comparer);

        leftSet.ExceptWith(rightColl);
        res.left = leftSet;

        rightSet.ExceptWith(leftColl);
        res.right = rightSet;

        if (computeCommon)
        {
            var commonSet = new HashSet<T>(leftColl, comparer);
            commonSet.Intersect(rightColl);
            res.common = commonSet;
        }
        
        return res;
    }

    public static string CamelCaseToUnderscore(string s)
    {
        var tokens = Regex.Split(s, @"(?=[A-Z])");
        return string.Join("_", tokens.Where(x => x != string.Empty)
                                      .Select(x => x.ToLowerInvariant())
                                      .ToArray());
    }

    public static void Destroy(UnityEngine.Object obj)
    {
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(obj);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    public static Bounds ComputeLocalBounds(Canvas canvas)
    {
        var min = Vector3.one * float.MaxValue;
        var max = Vector3.one * float.MinValue;

        // find the ui bound in canvas space
        var children = canvas.GetComponentsInChildren<Graphic>().Select(x => x.rectTransform);
        foreach (RectTransform child in children)
        {
            var matToCanvas = canvas.transform.worldToLocalMatrix * child.localToWorldMatrix;
            var size = new Vector2(LayoutUtility.GetPreferredWidth(child), LayoutUtility.GetPreferredHeight(child));
            size = Vector2.Max(size, child.sizeDelta);
            var lowerLeft = matToCanvas.MultiplyPoint3x4(-Vector2.Scale(size, child.pivot));
            var upperRight = lowerLeft + matToCanvas.MultiplyVector(size);

            min = Vector3.Min(min, lowerLeft);
            max = Vector3.Max(max, upperRight);
        }

        return new Bounds((min + max) / 2, max - min);
    }

    public static Rect ComputeLocalRect(RectTransform parent, IEnumerable<RectTransform> transforms)
    {
        float panelLeft, panelRight, panelTop, panelBottom;
        panelLeft = panelBottom = float.MaxValue;
        panelRight = panelTop = float.MinValue;

        foreach (var trans in transforms)
        {
            Vector3 pos = parent.InverseTransformPoint(trans.position);
            panelLeft = Mathf.Min(pos.x, panelLeft);
            panelTop = Mathf.Max(pos.y, panelTop);
            panelRight = Mathf.Max(pos.x + trans.rect.size.x, panelRight);
            panelBottom = Mathf.Min(pos.y - trans.rect.size.y, panelBottom);
        }

        return new Rect(panelLeft, panelBottom, panelRight - panelLeft, panelTop - panelBottom);
    }

    public static void GotoHomeScene()
    {
        SceneDirector.ClearHistory();
        SceneDirector.Push("Lobby", saveCurSceneOnHistory: false);
    }
}


public class SetComparisonResult<T>
{
    public IEnumerable<T> left;
    public IEnumerable<T> common;
    public IEnumerable<T> right;
}

public static class RandomUtils
{
    public static Vector2 RandomVector2(Vector2 range)
    {
        return new Vector2(Range(0, range.x), Range(0, range.y));
    }

    /// <summary>
    /// return a random number in [a, b] if a &lt; b or in [b, a] otherwise
    /// </summary>
    public static float Range(float a, float b)
    {
        return UnityEngine.Random.Range(a < b ? a : b, a < b ? b : a);
    }
}
