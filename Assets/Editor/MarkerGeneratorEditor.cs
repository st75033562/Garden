using OpenCVForUnity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MarkerGeneratorEditor : EditorWindow
{
    class DictInfo
    {
        public int markerSize;
        public int dictSize;
        public int dictIndex;

        public DictInfo(int markerSize, int dictSize, int dictIndex)
        {
            this.markerSize = markerSize;
            this.dictSize = dictSize;
            this.dictIndex = dictIndex;
        }
    }

    // predefined dictionaries
    private static readonly DictInfo[] s_dicts = new DictInfo[] {
        new DictInfo(4, 50, Aruco.DICT_4X4_50),
        new DictInfo(4, 100, Aruco.DICT_5X5_100),
        new DictInfo(5, 50, Aruco.DICT_5X5_50),
        new DictInfo(5, 100, Aruco.DICT_5X5_100),
        new DictInfo(6, 50, Aruco.DICT_6X6_50),
        new DictInfo(6, 100, Aruco.DICT_6X6_100),
        new DictInfo(7, 50, Aruco.DICT_7X7_50),
        new DictInfo(7, 100, Aruco.DICT_7X7_100),
    };

    private int m_dictIndex = 0;
    private int m_imageSize = 128;
    private int m_markerNum = 20;
    private string m_outputPath = "";

    [MenuItem("Tools/Gen Markers...")]
    public static void Open()
    {
        GetWindow<MarkerGeneratorEditor>();
    }

    void OnGUI()
    {
        m_dictIndex = EditorGUILayout.Popup(
            "Dictionary",
            m_dictIndex,
            s_dicts.Select(x => string.Format("marker size: {0}x{0}, dict size: {1}", x.markerSize, x.dictSize)).ToArray());
        m_imageSize = EditorGUILayout.IntField("Image Size", m_imageSize);
        m_markerNum = EditorGUILayout.IntField("Marker Num", m_markerNum);
        m_outputPath = EditorGUILayout.TextField("Output Path", m_outputPath);

        if (GUILayout.Button("Generate"))
        {
            if (!Directory.Exists(m_outputPath))
            {
                string outputPath = EditorUtility.OpenFolderPanel("Select output path", Application.dataPath, "");
                if (outputPath != "")
                {
                    m_outputPath = outputPath;
                    Generate(s_dicts[m_dictIndex].dictIndex);
                }
            }
        }
    }

    void Generate(int dictIndex)
    {
        var dict = Aruco.getPredefinedDictionary(dictIndex);
        for (int i = 0; i < m_markerNum; ++i)
        {
            using (var image = new Mat())
            {
                Aruco.drawMarker(dict, i, m_imageSize, image);
                var tex = new Texture2D(image.cols(), image.width(), TextureFormat.RGBA32, false);
                OpenCVForUnity.Utils.matToTexture2D(image, tex);
                File.WriteAllBytes(Path.Combine(m_outputPath, i + ".png"), tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }
        }
    }
}
