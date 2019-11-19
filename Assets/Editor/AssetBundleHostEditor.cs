using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleHostEditor : EditorWindow
{
    private AssetBundleHostList m_hostList;

    [MenuItem("Tools/Bundle Host")]
    public static void Open()
    {
        EditorWindow.GetWindow<AssetBundleHostEditor>();
    }

    private void OnEnable()
    {
        m_hostList = AssetBundleHostList.Load();
        if (m_hostList == null)
        {
            m_hostList = new AssetBundleHostList();
        }
    }

    private void OnDisable()
    {
        m_hostList.Save();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < m_hostList.hosts.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.Toggle(i == m_hostList.enabledHostIndex, GUILayout.Width(20)))
            {
                m_hostList.enabledHostIndex = i;
            }
            else if (i == m_hostList.enabledHostIndex)
            {
                m_hostList.enabledHostIndex = -1;
            }
            m_hostList.hosts[i] = EditorGUILayout.TextField(m_hostList.hosts[i]);
            if (Event.current.keyCode == KeyCode.Return)
            {
                string host = m_hostList.hosts[i];
                host = host.Trim().Replace('\\', '/');
                if (!host.EndsWith("/"))
                {
                    host = host + "/";
                }
                if (Uri.IsWellFormedUriString(host, UriKind.Absolute))
                {
                    m_hostList.hosts[i] = host;
                }
            }
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                m_hostList.Remove(i);
                --i;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+"))
        {
            m_hostList.hosts.Add(string.Empty);
        }

        EditorGUILayout.EndVertical();
    }
}
