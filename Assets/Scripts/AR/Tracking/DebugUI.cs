using OpenCVForUnitySample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AR
{
    class DebugUI : MonoBehaviour
    {
        private readonly SortedDictionary<int, Vector3> m_markerPositions = new SortedDictionary<int, Vector3>();

        public MarkerTracker tracker
        {
            get;
            set;
        }

        IEnumerator Start()
        {
            while (true)
            {
                if (tracker)
                {
                    m_markerPositions.Clear();
                    foreach (var marker in tracker.Markers)
                    {
                        var m = marker.WorldMatrix;
                        ARUtils.ExtractTranslationFromMatrix(ref m);
                        m_markerPositions[marker.Id] = ARUtils.ExtractTranslationFromMatrix(ref m);
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        void OnGUI()
        {
            if (!tracker)
            {
                return;
            }

            float scale = Mathf.Max(Screen.width / 1344.0f, Screen.height / 750.0f);
            GUI.matrix = Matrix4x4.Scale(scale * Vector3.one);
            GUI.skin.label.fontSize = 20;

            GUILayout.BeginArea(new UnityEngine.Rect(0, 100, Screen.width, Screen.height));
            foreach (var marker in m_markerPositions)
            {
                var pos = marker.Value;
                GUILayout.Label(string.Format("{0}: {1:F2} {2:F2} {3:F2}", marker.Key, pos.x, pos.y, pos.z));
            }
            GUILayout.EndArea();
        }
    }
}
