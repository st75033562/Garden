using UnityEngine;

namespace Gameboard
{
    [ObjectActionParameter(1)]
    public class ObjectActionSetFrame : ObjectAction
    {
        [SerializeField]
        private int m_columns;

        [SerializeField]
        private int m_rows;

        [SerializeField]
        private Renderer m_render;

        [SerializeField]
        private bool m_applyUVScale; // if false, assume the initial uv is in the first frame

        [SerializeField]
        private int m_defaultFrameIndex;

        public class Config
        {
            [FieldValue(-1)]
            public int frameIndex;
        }

        public override void Execute(object o, params string[] args)
        {
            int frameIndex = ((Config)o).frameIndex;
            if (frameIndex == -1 && args.Length > 0)
            {
                if (!int.TryParse(args[0], out frameIndex))
                {
                    return;
                }
            }

            SetFrameIndex(frameIndex);
        }

        public void SetFrameIndex(int index)
        {
            if (index >= 0  && index < m_columns * m_rows)
            {
                // frames are ordered from left to right and from top to bottom
                var scale = new Vector2(1.0f / m_columns, 1.0f / m_rows);
                if (m_applyUVScale)
                {
                    m_render.material.mainTextureScale = scale;
                }

                float offsetY = m_applyUVScale ? 1.0f - (index / m_rows + 1) * scale.y : -index / m_rows * scale.y;
                m_render.material.mainTextureOffset = new Vector2(index % m_columns * scale.x, offsetY);
            }
        }

        public override void Stop()
        {
            SetFrameIndex(m_defaultFrameIndex);
        }
    }
}
