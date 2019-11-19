using OpenCVForUnity;
using OpenCVForUnitySample;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace AR
{
    public enum TrackingMode
    {
        WorldCenter,
        FixedCamera,
    }

    public class KalmanFilterParams
    {
        public float processNoise;
        public float measurementNoise;
        public float errorCovPost;

        public KalmanFilterParams() { }

        public KalmanFilterParams(KalmanFilterParams rhs)
        {
            processNoise = rhs.processNoise;
            measurementNoise = rhs.measurementNoise;
            errorCovPost = rhs.errorCovPost;
        }
    }

    public enum TrackingFilterType
    {
        Kalman,
        Exponentail
    }

    public class CameraCalibrationConfig
    {
        public double fx; // focal length in pixels
        public double fy;
        public double cx; // principal point offset
        public double cy;
        public double[] distCoeff; // distortion coefficients
    }

    [RequireComponent(typeof(ArWebCamTextureToMatHelper))]
    public class MarkerTracker : MonoBehaviour
    {
        public event Action OnTrackingUpdated;

        [SerializeField]
        private int m_dictionaryId = 0;

        [SerializeField]
        private bool m_useCustomDict;

        [SerializeField]
        private int m_markerSize;

        [SerializeField]
        private int m_dictionarySize;

        [SerializeField]
        private Camera m_videoPlaneCamera = null;

        [SerializeField]
        private Camera m_ARCamera = null;

        [SerializeField]
        private float m_markerLength = 1;

        [SerializeField]
        private int m_worldCenterMarkerId = 0; // valid in world center mode

        [SerializeField]
        private TrackingMode m_trackingMode = TrackingMode.FixedCamera;

        [SerializeField]
        private bool m_debugDraw;

        [Serializable]
        public class CameraResolution
        {
            public int width;
            public int height;

            public CameraResolution(int w, int h)
            {
                width = w;
                height = h;
            }
        }

        [SerializeField]
        private CameraResolution m_mobileCamRes = new CameraResolution(640, 480);

        [SerializeField]
        private CameraResolution m_pcCamRes = new CameraResolution(640, 480);
        
        private static readonly KalmanFilterParams s_poseFilterParams = new KalmanFilterParams {
            processNoise = 0.1f,
            measurementNoise = 0.1f,
            errorCovPost = 1f
        };

        private static readonly KalmanFilterParams s_cornerFilterParams = new KalmanFilterParams {
            processNoise = 0.1f,
            measurementNoise = 0.1f,
            errorCovPost = 1f
        };

        private static readonly float[] s_rotationFilterWeights = new float[] {
            1, 2, 3, 4
        };

        private const float s_expSmoothFactor = 0.7f;

        // how many frames to keep after losing tracking of the marker
        private const int MaxTTLAfterTrackingLost = 20;

        private class TrackingState
        {
            public TrackedMarker marker;
            public CornerFilter cornerFilter;
            public PoseFilter poseFilter;
            public PoseFilter worldMatrixFilter;

            public float deltaTimeCorner;
            public float deltaTimePose;

            public int lostFrames; // video frames since tracking was lost

            public Vector2 markerCenter; // current geometric center of 4 corners
            public int index; // index into the current ids array
            public float minDistance; // min distance from current center to the candidate's center
            public Vector2 candidateCenter; // the center of the best candidate

            public bool hasValidPose;

            public void Dispose()
            {
                cornerFilter.Dispose();
                poseFilter.Dispose();
                worldMatrixFilter.Dispose();
            }

            public bool IsAlive
            {
                get { return lostFrames < MaxTTLAfterTrackingLost; }
            }
        }

        private readonly List<int> m_staleIds = new List<int>();
        private readonly Dictionary<int, TrackingState> m_trackingStates = new Dictionary<int, TrackingState>();
        private readonly Dictionary<int, TrackingState> m_lostStates = new Dictionary<int, TrackingState>();
        private float m_timeSinceLastDetection;

        private ArWebCamTextureToMatHelper m_webCamTextureToMatHelper;

        private Mat m_camMatrix;
        private MatOfDouble m_distCoeffs;
        private Mat m_rgbaMat;
        private Mat m_rotMat;

        private static readonly Matrix4x4 s_invertYM;
        private static readonly Matrix4x4 s_swapYZM;

        private DetectorParameters m_detectorParams;
        private Dictionary m_dictionary;

        // tracking is enabled by default
        private bool m_trackingStarted = true;

        private DetectionTask m_detectionTask;

        private bool m_inited;

        private Texture2D m_debugTexture;

        private TrackingFilterType m_poseFilterType = Debug.poseFilterType;
        private KalmanFilterParams m_poseFilterConfig = new KalmanFilterParams(s_poseFilterParams);
        private float m_poseExpSmoothFactor = s_expSmoothFactor;
        private CameraCalibrationConfig m_calibrationConfig;

        static MarkerTracker()
        {
            s_invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));

            // unity is left-handed while OpenCV is right-handed, convert from unity to OpenCV
            s_swapYZM.m00 = 1.0f;
            s_swapYZM.m12 = 1.0f;
            s_swapYZM.m21 = 1.0f;
            s_swapYZM.m33 = 1.0f;
        }

        void Start()
        {
            // make the video plane fullscreen
            var height = m_videoPlaneCamera.orthographicSize * 2;
            var width = m_videoPlaneCamera.aspect * height;
            transform.localScale = new Vector3(width, height);
            GetComponent<Renderer>().material.color = Color.black;

#if !DEVELOP
            m_debugDraw = false;
#endif
            m_webCamTextureToMatHelper = gameObject.GetComponent<ArWebCamTextureToMatHelper>();
            if (Application.isMobilePlatform)
            {
                m_webCamTextureToMatHelper.requestWidth = m_mobileCamRes.width;
                m_webCamTextureToMatHelper.requestHeight = m_mobileCamRes.height;
            }
            else
            {
                m_webCamTextureToMatHelper.requestWidth = m_pcCamRes.width;
                m_webCamTextureToMatHelper.requestHeight = m_pcCamRes.height;
            }
            m_webCamTextureToMatHelper.Init();

            ApplicationEvent.onResolutionChanged += OnResolutionChanged;
        }

        void OnDestroy()
        {
            if (m_webCamTextureToMatHelper)
            {
                m_webCamTextureToMatHelper.Dispose();
            }
            Cleanup();
            ApplicationEvent.onResolutionChanged -= OnResolutionChanged;
        }

        private void OnResolutionChanged()
        {
            if (m_inited)
            {
                UpdateCameraSettings();
            }
        }

        public IEnumerable<TrackedMarker> Markers
        {
            get
            {
                foreach (var v in m_trackingStates.Values.Concat(m_lostStates.Values))
                {
                    if (v.hasValidPose)
                    {
                        yield return v.marker;
                    }
                }
            }
        }

        public TrackedMarker GetMarker(int markerId)
        {
            var marker = GetMarker(m_trackingStates, markerId);
            if (marker == null)
            {
                marker = GetMarker(m_lostStates, markerId);
            }
            return marker;
        }

        private TrackedMarker GetMarker(Dictionary<int, TrackingState> states, int markerId)
        {
            TrackingState state;
            states.TryGetValue(markerId, out state);
            return state != null && state.hasValidPose ? state.marker : null;
        }

        public void OnWebCamTextureToMatHelperInited()
        {
            UnityEngine.Debug.Log("OnWebCamTextureToMatHelperInited");

            ResetVideoPlane();

            //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            UpdateCameraSettings();

            m_rotMat = new Mat(3, 3, CvType.CV_64FC1);

            m_detectorParams = DetectorParameters.create();
            // this substantially improves the tracking
            m_detectorParams.set_doCornerRefinement(true);

            if (m_useCustomDict)
            {
                m_dictionary = Aruco.custom_dictionary(m_dictionarySize, m_markerSize);
            }
            else
            {
                m_dictionary = Aruco.getPredefinedDictionary(m_dictionaryId);
            }

            Mat webCamTextureMat = m_webCamTextureToMatHelper.GetMat();
            m_detectionTask = new DetectionTask(webCamTextureMat.rows(), webCamTextureMat.cols());
            m_inited = true;
        }

        private void ResetVideoPlane()
        {
            DestroyDebugTexture();

            var renderer = GetComponent<Renderer>();
            var webCamTex = m_webCamTextureToMatHelper.GetWebCamTexture();
            if (m_debugDraw)
            {
                m_debugTexture = new Texture2D(webCamTex.width, webCamTex.height, TextureFormat.RGBA32, false);
                renderer.material.mainTexture = m_debugTexture;
            }
            else
            {
                renderer.material.mainTexture = webCamTex;

                var yScale = webCamTex.videoVerticallyMirrored ? -webCamTex.height : webCamTex.height;
                transform.localScale = new Vector3(webCamTex.width, yScale, 1);
                transform.localRotation = Quaternion.AngleAxis(webCamTex.videoRotationAngle, Vector3.back);
            }
            renderer.material.color = Color.white;
        }

        private void UpdateCameraSettings()
        {
            Mat webCamTextureMat = m_webCamTextureToMatHelper.GetMat();

            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale > heightScale)
            {
                m_videoPlaneCamera.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                m_videoPlaneCamera.orthographicSize = height / 2;
            }

            CameraCalibrationConfig calibConfig = m_calibrationConfig;
            if (calibConfig == null)
            {
                int max_d = (int)Mathf.Max(width, height);
                calibConfig = new CameraCalibrationConfig {
                    fx = max_d,
                    fy = max_d,
                    cx = width / 2.0f,
                    cy = height / 2.0f,
                };
            }

            SetCameraCalibrationInternal(calibConfig);
        }

        public void OnWebCamTextureToMatHelperDisposed()
        {
            //Debug.Log("OnWebCamTextureToMatHelperDisposed");
            Cleanup();
        }

        private void Cleanup()
        {
            Utils.Dispose(ref m_detectionTask);
            ResetTrackingStates();
            Utils.Dispose(ref m_rotMat);
            DestroyDebugTexture();

            m_timeSinceLastDetection = 0;
            m_inited = false;
        }

        private void DestroyDebugTexture()
        {
            if (m_debugTexture)
            {
                Destroy(m_debugTexture);
                m_debugTexture = null;
            }
        }

        public void StartTracking()
        {
            m_trackingStarted = true;
        }

        public void StopTracking()
        {
            m_timeSinceLastDetection = 0;
            m_trackingStarted = false;
        }

        public void ResetTrackingStates()
        {
            foreach (var state in m_trackingStates.Values)
            {
                state.Dispose();
            }
            m_trackingStates.Clear();

            foreach (var state in m_lostStates.Values)
            {
                state.Dispose();
            }
            m_lostStates.Clear();
        }

        public float MarkerLength
        {
            get { return m_markerLength; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_markerLength = value;
            }
        }

        public int WorldCenterMarkerId
        {
            get { return m_worldCenterMarkerId; }
            set
            {
                if (m_worldCenterMarkerId != value)
                {
                    m_worldCenterMarkerId = value;
                    ResetTrackingStates();
                }
            }
        }

        public TrackingMode TrackingMode
        {
            get { return m_trackingMode; }
            set
            {
                if (m_trackingMode != value)
                {
                    m_trackingMode = value;
                    ResetTrackingStates();
                }
            }
        }

        public bool DebugDraw
        {
            get { return m_debugDraw; }
            set
            {
                if (m_debugDraw != value)
                {
                    m_debugDraw = value;
                    if (m_inited)
                    {
                        ResetVideoPlane();
                    }
                }
            }
        }

        public Camera VideoCamera
        {
            get { return m_videoPlaneCamera; }
        }

        void Update()
        {
            if (!m_inited)
            {
                return;
            }

            if (!m_trackingStarted)
            {
                if (m_detectionTask != null && m_webCamTextureToMatHelper.didUpdateThisFrame())
                {
                    UpdateDebugTexture(true);
                }
                return;
            }

            m_timeSinceLastDetection += Time.unscaledDeltaTime;

            if (m_webCamTextureToMatHelper.didUpdateThisFrame())
            {
                m_detectionTask.timeSinceLastDetection = m_timeSinceLastDetection;
                m_rgbaMat = m_webCamTextureToMatHelper.GetMat();

                Profiler.BeginSample("DetectMarkers");
                DetectMarkers(m_detectionTask);
                Profiler.EndSample();

                Profiler.BeginSample("ProcessDetectedMarkers");
                ProcessDetectedMarkers(m_detectionTask);
                Profiler.EndSample();

                UpdateDebugTexture(false);

                m_timeSinceLastDetection = 0;
            }
        }

        private void UpdateDebugTexture(bool fetchFromWebcamTexture)
        {
            if (m_debugDraw)
            {
                if (fetchFromWebcamTexture)
                {
                    Imgproc.cvtColor(m_webCamTextureToMatHelper.GetMat(), m_detectionTask.rgbMat, Imgproc.COLOR_RGBA2RGB);
                }

                OpenCVForUnity.Utils.matToTexture2D(m_detectionTask.rgbMat, 
                    m_debugTexture, m_webCamTextureToMatHelper.GetBufferColors());
            }
        }

        private void DetectMarkers(object state)
        {
            var task = (DetectionTask)state;

            Profiler.BeginSample("cvtColor");
            Imgproc.cvtColor(m_rgbaMat, task.rgbMat, Imgproc.COLOR_RGBA2RGB);
            Profiler.EndSample();

            Profiler.BeginSample("detectMarker");
            // detect markers and estimate pose
            Aruco.detectMarkers(task.rgbMat, m_dictionary, task.corners, task.ids, m_detectorParams, task.rejected);
            Profiler.EndSample();

            task.state = DetectionState.Completed;
        }

        private void ProcessDetectedMarkers(DetectionTask task)
        {
            Profiler.BeginSample("PrepareTrackingStates");
            PrepareTrackingStates(task);
            Profiler.EndSample();

            Profiler.BeginSample("FilterCorners");
            FilterCorners(task.corners);
            Profiler.EndSample();

            if (task.ids.total() > 0)
            {
                Profiler.BeginSample("Aruco.estimatePoseSingleMarkers");
                Aruco.estimatePoseSingleMarkers(task.corners, m_markerLength, m_camMatrix, m_distCoeffs, task.rvecs, task.tvecs);
                Profiler.EndSample();

                if (m_debugDraw)
                {
                    Aruco.drawDetectedMarkers(task.rgbMat, task.corners, task.ids, new Scalar(0, 255, 0));
                }

                Profiler.BeginSample("Post calculation");
                foreach (var state in m_trackingStates.Values)
                {
                    if (state.index == -1)
                    {
                        continue;
                    }

                    Calib3d.Rodrigues(task.rvecs.row(state.index), m_rotMat);

                    if (m_debugDraw)
                    {
                        using (Mat rvec = new Mat(task.rvecs, new OpenCVForUnity.Rect(0, state.index, 1, 1)))
                        using (Mat tvec = new Mat(task.tvecs, new OpenCVForUnity.Rect(0, state.index, 1, 1)))
                        {
                            Aruco.drawAxis(task.rgbMat, m_camMatrix, m_distCoeffs, rvec, tvec, m_markerLength);
                        }
                    }

                    var poseMatrix = Utils.TR(m_rotMat, task.tvecs.get(state.index, 0));
                    if (!IsValidPose(ref poseMatrix))
                    {
                        if (Debug.dumpInvalidPoseMarkerId == state.marker.Id)
                        {
                            UnityEngine.Debug.LogFormat("invalid pose: {0}\n{1}", state.marker.Id, poseMatrix);
                        }

                        state.index = -1;
                        continue;
                    }

                    state.marker.RawPoseMatrix = poseMatrix;
                    state.marker.PoseMatrix = state.poseFilter.Filter(ref poseMatrix, state.deltaTimePose);

                    if (m_trackingMode == TrackingMode.FixedCamera)
                    {
                        state.hasValidPose = true;
                        state.marker.WorldMatrix = 
                            m_ARCamera.transform.localToWorldMatrix * s_invertYM * state.marker.PoseMatrix * s_swapYZM;
                    }
                }

                if (m_trackingMode == TrackingMode.WorldCenter && m_trackingStates.ContainsKey(WorldCenterMarkerId))
                {
                    var worldCenterState = m_trackingStates[WorldCenterMarkerId];

                    // transform of world center marker is always identity
                    worldCenterState.marker.WorldMatrix = Matrix4x4.identity;
                    worldCenterState.hasValidPose = true;

                    Matrix4x4 inversePose = worldCenterState.marker.PoseMatrix.inverse;

                    // find the camera matrix in the world space
                    var camWorldMatrix = s_swapYZM * inversePose * s_invertYM;
                    
                    m_ARCamera.transform.position = ARUtils.ExtractTranslationFromMatrix(ref camWorldMatrix);
                    m_ARCamera.transform.rotation = ARUtils.ExtractRotationFromMatrix(ref camWorldMatrix);

                    if (Debug.dumpPoseMarkerId == WorldCenterMarkerId)
                    {
                        UnityEngine.Debug.LogFormat("marker: {0}\n{1}", 0, worldCenterState.marker.RawPoseMatrix);
                    }

                    foreach (var state in m_trackingStates.Values)
                    {
                        if (state.marker.Id != WorldCenterMarkerId && state.index != -1)
                        {
                            state.hasValidPose = true;

                            Matrix4x4 worldMatrix;
                            worldMatrix = s_swapYZM * inversePose * state.marker.PoseMatrix * s_swapYZM;

                            if (Debug.dumpPoseMarkerId == state.marker.Id)
                            {
                                var cvRawWorldM = worldCenterState.marker.RawPoseMatrix.inverse * state.marker.RawPoseMatrix;
                                UnityEngine.Debug.LogFormat("marker: {0}\n{1}", state.marker.Id, cvRawWorldM);
                            }

                            // orthonormalize to make sure y is vertical
                            var y = Vector3.up;
                            var x = MathUtils.GetX(ref worldMatrix);
                            var z = MathUtils.GetZ(ref worldMatrix);
                            Vector3.OrthoNormalize(ref y, ref x, ref z);
                            MathUtils.SetX(ref worldMatrix, x);
                            MathUtils.SetY(ref worldMatrix, y);
                            MathUtils.SetZ(ref worldMatrix, z);

                            state.marker.WorldMatrix = state.worldMatrixFilter.Filter(ref worldMatrix, state.deltaTimePose);
                        }
                    }
                }
                Profiler.EndSample();

                foreach (var state in m_trackingStates.Values)
                {
                    if (state.index != -1)
                    {
                        state.deltaTimePose = 0;
                    }
                }
            }

            if (OnTrackingUpdated != null)
            {
                OnTrackingUpdated();
            }
        }

        private bool IsValidPose(ref Matrix4x4 pose)
        {
            // opencv camera coordinate system
            // https://docs.opencv.org/2.4/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html
            // marker coordinate system
            // https://docs.opencv.org/3.1.0/d5/dae/tutorial_aruco_detection.html

            bool validPose = true;
            // get the world up's y in the camera's coordinate system
            float worldUpY = pose.m12;
            // if the physical camera is right side up, then the world up's y should be negative
            // NOTE: on pc, we assume the camera is always right side up
            if ((!Application.isMobilePlatform || Input.acceleration.y < 0) && worldUpY > 0)
            {
                validPose = false;
            }
            // if the physical camera is upside down, then the world up's y should be positive
            else if (Input.acceleration.y > 0 && worldUpY < 0)
            {
                validPose = false;
            }
            return validPose;
        }

        private Vector2 CenterOfCorners(Mat corners)
        {
            const int CornerCount = 4;
            Vector2 center = Vector2.zero;
            for (int i = 0; i < CornerCount; ++i)
            {
                center += corners.ToVector2_11(0, i);
            }
            return center / CornerCount;
        }

        private void PrepareTrackingStates(DetectionTask task)
        {
            foreach (var state in m_trackingStates.Values)
            {
                state.index = -1;
                state.minDistance = float.MaxValue;
                // accumulated, for markers which are detected in this frame, the value will be reset
                state.deltaTimeCorner += task.timeSinceLastDetection;
                state.deltaTimePose += task.timeSinceLastDetection;
            }

            int count = (int)task.ids.total();
            // update or create tracking states
            for (int i = 0; i < count; ++i)
            {
                int id = (int)task.ids.get(i, 0)[0];

                TrackingState state;
                if (!m_trackingStates.TryGetValue(id, out state))
                {
                    if (m_lostStates.TryGetValue(id, out state))
                    {
                        state.lostFrames = 0;
                        state.marker.IsLost = false;
                        m_lostStates.Remove(id);
                    }
                    else
                    {
                        state = new TrackingState();
                        state.marker = new TrackedMarker(id);
                        state.poseFilter = CreatePoseFilter();
                        state.worldMatrixFilter = CreatePoseFilter();
                        state.cornerFilter = CreateCornerFilter();
                    }
                    // a new marker, we assume this is the best candidate
                    state.markerCenter = state.candidateCenter = CenterOfCorners(task.corners[i]);
                    state.index = i;
                    m_trackingStates.Add(id, state);
                }
                else
                {
                    state.lostFrames = 0;
                    // check if the candidate center is closer to the last marker center
                    var candidateCenter = CenterOfCorners(task.corners[i]);
                    var dist = Vector2.SqrMagnitude(candidateCenter - state.markerCenter);
                    if (dist < state.minDistance)
                    {
                        state.candidateCenter = candidateCenter;
                        state.index = i;
                        state.minDistance = dist;
                    }
                }
            }

            // collect stale states and update marker centers
            foreach (var state in m_trackingStates.Values)
            {
                if (state.index != -1)
                {
                    // save the best candidate
                    state.markerCenter = state.candidateCenter;
                }
                else
                {
                    ++state.lostFrames;
                    if (!state.IsAlive)
                    {
                        state.marker.IsLost = true;
                        m_staleIds.Add(state.marker.Id);
                    }
                }
            }

            // remove all stale states
            for (int i = 0; i < m_staleIds.Count; ++i)
            {
                var lostState = m_trackingStates[m_staleIds[i]];
                m_lostStates.Add(m_staleIds[i], lostState);
                m_trackingStates.Remove(m_staleIds[i]);
            }
            m_staleIds.Clear();
        }

        private PoseFilter CreatePoseFilter()
        {
            IVector3Filter posFilter;
            IQuaternionFilter rotationFilter = new SimpleQuaternionFilter(s_rotationFilterWeights);
            if (m_poseFilterType == TrackingFilterType.Kalman)
            {
                posFilter = new Vector3KalmanFilter();
                ConfigureKalmanFilter((Vector3KalmanFilter)posFilter, s_poseFilterParams);
            }
            else
            {
                posFilter = new ExponentialVector3Filter { smoothFactor = s_expSmoothFactor };
            }

            return new PoseFilter(posFilter, rotationFilter);
        }

        private void ConfigureKalmanFilter(LinearKalmanFilter filter, KalmanFilterParams param)
        {
            filter.ProcessNoise = param.processNoise;
            filter.MeasurementNoise = param.measurementNoise;
            filter.ErrorCovPost = param.errorCovPost;
        }

        private CornerFilter CreateCornerFilter()
        {
            return new CornerFilter(() => {
                var filter = new Vector2KalmanFilter();
                ConfigureKalmanFilter(filter, s_cornerFilterParams);
                return filter;
            });
        }

        private void FilterCorners(List<Mat> corners)
        {
            foreach (var state in m_trackingStates.Values)
            {
                if (state.index != -1)
                {
                    state.cornerFilter.Filter(corners[state.index], state.deltaTimeCorner);
                    state.deltaTimeCorner = 0;
                }
            }
        }

        // set the camera calibration config, if null, no calibration is performed
        public void SetCameraCalibration(CameraCalibrationConfig config)
        {
            m_calibrationConfig = config;
            if (!m_inited)
            {
                return;
            }

            if (config == null)
            {
                m_webCamTextureToMatHelper.Init();
                return;
            }

            SetCameraCalibrationInternal(config);
        }

        private void SetCameraCalibrationInternal(CameraCalibrationConfig config)
        {
            if (m_camMatrix == null)
            {
                m_camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            }

            if (m_distCoeffs == null)
            {
                m_distCoeffs = new MatOfDouble();
            }
            m_distCoeffs.fromArray(config.distCoeff != null ? config.distCoeff : new double[4]);

            m_camMatrix.put(0, 0, config.fx);
            m_camMatrix.put(0, 1, 0);
            m_camMatrix.put(0, 2, config.cx);
            m_camMatrix.put(1, 0, 0);
            m_camMatrix.put(1, 1, config.fy);
            m_camMatrix.put(1, 2, config.cy);
            m_camMatrix.put(2, 0, 0);
            m_camMatrix.put(2, 1, 0);
            m_camMatrix.put(2, 2, 1.0f);

            var mat = m_webCamTextureToMatHelper.GetMat();
            var width = mat.width();
            var height = mat.height();
            float viewportWidth, viewportHeight;

            if ((float)Screen.width / width > (float)Screen.height / height)
            {
                viewportWidth = width;
                viewportHeight = width / m_ARCamera.aspect;
            }
            else
            {
                viewportWidth = height * m_ARCamera.aspect;
                viewportHeight = height;
            }

            var near = m_ARCamera.nearClipPlane;
            var far = m_ARCamera.farClipPlane;

            // update the projection matrix to take into account the calibration
            var projMatrix = new Matrix4x4();
            projMatrix.m00 = (float)(2.0f * config.fx / viewportWidth);
            projMatrix.m02 = (float)((-2.0f * config.cx + width) / viewportWidth);
            projMatrix.m11 = (float)(2.0f * config.fy / viewportHeight);
            projMatrix.m12 = (float)((2.0f * config.cy - height) / viewportHeight);
            projMatrix.m22 = -(far + near) / (far - near);
            projMatrix.m23 = -(2.0f * far * near) / (far - near);
            projMatrix.m32 = -1.0f;

            m_ARCamera.projectionMatrix = projMatrix;
        }

        #region debugging

        /// <summary>
        /// changes to the pose filter params does not affect existing tracked marker
        /// </summary>
        public KalmanFilterParams PoseKalmanFilterParams
        {
            get { return m_poseFilterConfig; }
            set { m_poseFilterConfig = value ?? new KalmanFilterParams(s_poseFilterParams); }
        }

        public float PoseExpSmoothFactor
        {
            get { return m_poseExpSmoothFactor; }
            set { m_poseExpSmoothFactor = Mathf.Clamp01(value); }
        }

        public TrackingFilterType PoseFilterType
        {
            get { return m_poseFilterType; }
            set
            {
                if (m_poseFilterType != value)
                {
                    m_poseFilterType = value;
                    ResetTrackingStates();
                }
            }
        }

        #endregion
    }

}