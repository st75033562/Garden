using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RobotSimulation
{
    [CustomEditor(typeof(CameraManager))]
    class CameraManagerEditor : Editor
    {
        private SerializedProperty m_tweenDuration;
        private SerializedProperty m_topCamera;
        private SerializedProperty m_normalCamera;

        protected virtual void OnEnable()
        {
            m_tweenDuration = serializedObject.FindProperty("tweenDuration");
            m_topCamera = serializedObject.FindProperty("topCamera");
            m_normalCamera = serializedObject.FindProperty("normalCamera");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_tweenDuration);
            EditorGUILayout.PropertyField(m_topCamera);
            EditorGUILayout.PropertyField(m_normalCamera);

            EditorGUILayout.BeginHorizontal();

            var cameraManager = (CameraManager)serializedObject.targetObject;
            for (var type = CameraType.Normal; type < CameraType.Max; ++type)
            {
                GUI.backgroundColor = cameraManager.cameraType == type ? Color.green : Color.white;
                if (GUILayout.Button(type.ToString()))
                {
                    if (Application.isPlaying)
                    {
                        cameraManager.ActivateCamera(type);
                    }
                    else
                    {
                        cameraManager.SetCamera(type);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
