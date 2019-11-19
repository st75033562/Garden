using UnityEngine;
using System;

namespace RockVR.Video
{
    /// <summary>
    /// Config setup for video related path.
    /// </summary>
    public class PathConfig
    {
        public static string persistentDataPath = Application.persistentDataPath;
        public static string streamingAssetsPath = Application.streamingAssetsPath;
        public static string myDocumentsPath = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);

        static PathConfig()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            saveFolder = persistentDataPath + "/RockVR/Video/";
#else
            saveFolder = myDocumentsPath + "/RockVR/Video/";
#endif
        }

        /// <summary>
        /// The video folder, save recorded video.
        /// </summary>
        public static string saveFolder
        {
            get;
            set;
        }

        /// <summary>
        /// The ffmpeg path.
        /// </summary>
        public static string ffmpegPath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return ApplicationUtils.externalToolsPath + "/RockVR/FFmpeg/Windows/ffmpeg.exe";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return ApplicationUtils.externalToolsPath + "/RockVR/FFmpeg/OSX/ffmpeg";
#else
                return "";
#endif
            }
        }
        ///// <summary>
        ///// The <c>YoutubeUploader</c> script path.
        ///// </summary>
        //public static string youtubeUploader
        //{
        //    get
        //    {
        //        return streamingAssetsPath + "/RockVR/Scripts/YoutubeUploader.py";
        //    }
        //}

        /// <summary>
        /// The Spatial Media Metadata Injector path.
        /// </summary>
        public static string injectorPath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return ApplicationUtils.externalToolsPath + "/RockVR/Spatial Media Metadata Injector/Windows/Spatial Media Metadata Injector.exe";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return ApplicationUtils.externalToolsPath + "/RockVR/Spatial Media Metadata Injector/OSX/Spatial Media Metadata Injector.app";
#else
                return "";
#endif
            }
        }
    }
}