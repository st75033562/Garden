using RockVR.Video;
using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ThumbnailUtils
{
    /// <summary>
    /// generate a thumbnail of the given video, return the path to the thumbnail
    /// </summary>
    /// <param name="videoPath"></param>
    /// <param name="time">time point at which to extract the thumbnail</param>
    /// <param name="width">the width of the thumbnail</param>
    public static bool Generate(string videoPath, string outputPath, int time, int width)
    {
        try
        {
            FileUtils.createParentDirectory(outputPath);

#if UNITY_STANDALONE || UNITY_EDITOR
            videoPath = Utils.QuoteArgument(videoPath);
            outputPath = Utils.QuoteArgument(outputPath);
            // use ffmpeg to generate the thumbnail
            string arguments = string.Format("-ss {0} -i {1} -y -vf \"scale=min({2}\\,iw):-1\" -f image2 {3}", 
                time, videoPath, width > 0 ? width : int.MaxValue, outputPath);
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = PathConfig.ffmpegPath;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
                return proc.ExitCode == 0;
            }
#elif UNITY_ANDROID
            using (var util = new AndroidJavaClass("com.activ8.utility.Thumbnail"))
            {
                return util.CallStatic<bool>("create", videoPath, outputPath, time, width, 100, false);
            }
#elif UNITY_IPHONE
            return false;
#else
#error Unsupported platform
#endif
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
}
