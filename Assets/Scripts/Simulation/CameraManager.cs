using DG.Tweening;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RobotSimulation
{
    public enum CameraType
    {
        Normal,
        Top,
        Max,
    }

    public class CameraManager : MonoBehaviour
    {
        public float tweenDuration = 0.5f;

        public Camera topCamera;
        public Camera normalCamera;

        private Camera m_camera;

        public CameraType m_cameraType = CameraType.Normal;

        void Awake()
        {
            // for now, we assume all cameras have same settings, no effects
            m_camera = Instantiate(GetCamera(m_cameraType), transform);
            m_camera.enabled = true;

            topCamera.enabled = false;
            normalCamera.enabled = false;
        }

        public bool renderingOn
        {
            get { return m_camera.enabled; }
            set
            {
                m_camera.enabled = value;
            }
        }

        private Camera GetCamera(CameraType type)
        {
            switch (m_cameraType)
            {
            case CameraType.Normal:
                return normalCamera;

            case CameraType.Top:
                return topCamera;

            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public Camera currentCamera
        {
            get { return m_camera; }
        }

        public CameraType cameraType
        {
            get { return m_cameraType; }
        }

        public void ActivateCamera(CameraType type, bool skipAnimation = false, Camera targetCam = null)
        {
            m_cameraType = type;

            if (targetCam == null)
            {
                targetCam = GetCamera(m_cameraType);
            }
            
            if (!skipAnimation)
            {
                m_camera.transform.DOMove(targetCam.transform.position, tweenDuration).SetUpdate(true);
                m_camera.transform.DORotateQuaternion(targetCam.transform.rotation, tweenDuration).SetUpdate(true);
            }
            else
            {
                m_camera.transform.position = targetCam.transform.position;
                m_camera.transform.rotation = targetCam.transform.rotation;
            }
        }

#if UNITY_EDITOR
        public void SetCamera(CameraType type)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Change Camera Type");

            var oldCamera = GetCamera(m_cameraType);
            Undo.RecordObject(oldCamera, "Disable Camera: " + oldCamera.name);
            oldCamera.enabled = false;

            Undo.RecordObject(this, "Set Camera Type: " + type);
            m_cameraType = type;

            var currentCamera = GetCamera(m_cameraType);
            Undo.RecordObject(currentCamera, "Enable Camera: " + currentCamera.name);
            currentCamera.enabled = true;
        }
#endif

    }
}
