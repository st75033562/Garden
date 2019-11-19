using OpenCVForUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR
{
    interface IVideoStream
    {
        // return true if the frame was updated
        bool DidUpdateThisFrame { get; }

        // true if the stream is playing
        bool IsPlaying { get; }

        Mat FrameMat { get; }
    }
}
