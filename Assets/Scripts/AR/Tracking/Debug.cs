using UnityEngine;

namespace AR
{
    public static class Debug
    {
        // debugging configurations
        public static readonly CVarEnum<TrackingFilterType> poseFilterType = CVarEnum.Of("ar_filter", TrackingFilterType.Kalman);
        public static readonly CVarInt dumpPoseMarkerId = new CVarInt("ar_dump_marker_pose", -1);
        public static readonly CVarInt dumpInvalidPoseMarkerId = new CVarInt("ar_dump_marker_invalid_pose", -1);

        static Debug()
        {
            CmdServer.Register("ar_debug_draw", OnDebugDraw);
            CmdServer.Register("ar_reset_webcam", OnResetWebCam);
            CmdServer.Register("ar_debug_ui", OnDebugUI);
            CmdServer.Register("ar_calibrate", OnCalibrate);
        }

        private static string OnCalibrate(string[] args)
        {
            var tracker = Object.FindObjectOfType<MarkerTracker>();
            if (!tracker)
            {
                return "MarkerTracker not found";
            }

            if (args[0] != "0")
            {
                tracker.SetCameraCalibration(new CameraCalibrationConfig {
                    fx = 530.471019301914,
                    fy = 529.059082638099,
                    cx = 327.02178061534,
                    cy = 237.146668048833,
                    distCoeff = new double[] { 
                        0.389953356344204,
                        -2.64926691744518,
                        0.000344818756973158,
                        0.000252200029313853,
                        4.93578117451128,
                    },
                });
            }
            else
            {
                tracker.SetCameraCalibration(null);
            }
            return null;
		}

        static string OnDebugDraw(string[] args)
        {
            var tracker = Object.FindObjectOfType<MarkerTracker>();
            if (tracker)
            {
                tracker.DebugDraw = int.Parse(args[0]) != 0;
                return null;
            }
            else
            {
                return "MarkerTracker not found";
            }
        }

        private static string OnResetWebCam(string[] args)
        {
            var webCamHelper = Object.FindObjectOfType<ArWebCamTextureToMatHelper>();
            if (webCamHelper)
            {
                webCamHelper.requestWidth = int.Parse(args[0]);
                webCamHelper.requestHeight = int.Parse(args[1]);
                webCamHelper.Init();

                return null;
            }
            else
            {
                return "ArWebCamTextureToMatHelper not found";
            }
        }

        private static string OnDebugUI(string[] args)
        {
            bool enabled = int.Parse(args[0]) != 0;
            var debugUI = Object.FindObjectOfType<DebugUI>();
            if (enabled)
            {
                if (!debugUI)
                {
                    var markerTracker = Object.FindObjectOfType<MarkerTracker>();
                    if (markerTracker)
                    {
                        var go = new GameObject("ARDebugUI");
                        go.AddComponent<DebugUI>().tracker = markerTracker;
                    }
                    else
                    {
                        return "MarkerTracker not found";
                    }
                }
            }
            else
            {
                if (debugUI)
                {
                    Object.Destroy(debugUI.gameObject);
                }
            }
            return null;
        }

        // dummy for static initialization
        public static void Init() { }
    }
}
