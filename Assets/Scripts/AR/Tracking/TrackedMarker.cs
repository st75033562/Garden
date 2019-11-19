using System;
using UnityEngine;

namespace AR
{
    public class TrackedMarker
    {
        public TrackedMarker(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("id must be >= 0");
            }
            this.Id = id;

            PoseMatrix = Matrix4x4.identity;
            RawPoseMatrix = Matrix4x4.identity;
            WorldMatrix = Matrix4x4.identity;
        }

        // the marker id
        public int Id { get; private set; }

        #region relative pose matrices

        // return the filtered pose matrix
        public Matrix4x4 PoseMatrix { get; internal set; }

        // return the unfiltered pose matrix
        public Matrix4x4 RawPoseMatrix { get; internal set; }

        #endregion

        // the world matrix depends on the world center mode
        public Matrix4x4 WorldMatrix { get; internal set; }

        public bool IsLost { get; internal set; }
    }

}