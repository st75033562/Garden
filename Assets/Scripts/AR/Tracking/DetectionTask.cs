using OpenCVForUnity;
using System;
using System.Collections.Generic;

namespace AR
{
    enum DetectionState
    {
        Invalid,
        Processing,
        Completed,
    }

    class DetectionTask : IDisposable
    {
        public volatile DetectionState state = DetectionState.Invalid;
        // an monotonic increasing id
        // public int detectionId;

        public readonly Mat rgbMat;
        public readonly List<Mat> corners;
        public readonly Mat ids;
        public readonly Mat rvecs;
        public readonly Mat tvecs;
        public readonly List<Mat> rejected;
        public float timeSinceLastDetection;

        public DetectionTask(int rows, int cols)
        {
            rgbMat = new Mat(rows, cols, CvType.CV_8UC3);
            ids = new Mat();
            corners = new List<Mat>();
            rejected = new List<Mat>();
            rvecs = new Mat();
            tvecs = new Mat();
        }

        public bool HasMarkerId(int markerId)
        {
            for (int i = 0; i < ids.total(); ++i)
            {
                if ((int)ids.get(i, 0)[0] == markerId)
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            Utils.Dispose(rgbMat);
            Utils.Dispose(corners);
            Utils.Dispose(ids);
            Utils.Dispose(rvecs);
            Utils.Dispose(tvecs);
            Utils.Dispose(rejected);
        }
    }

}
