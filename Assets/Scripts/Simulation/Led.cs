using System;
using UnityEngine;

namespace RobotSimulation
{
    public enum LedColor
    {
    	Off,
    	Blue,
    	Green,
    	Cyan,
    	Red,
    	Magenta,
    	Yellow,
    	White
    }


    public static class LedColorExtensions
    {
        public static Color ToColor(this LedColor c)
        {
            switch (c)
            {
            case LedColor.Off:
                return Color.clear;

            case LedColor.Blue:
                return Color.blue;

            case LedColor.Green:
                return Color.green;

            case LedColor.Cyan:
                return Color.cyan;

            case LedColor.Red:
                return Color.red;

            case LedColor.Magenta:
                return Color.magenta;

            case LedColor.Yellow:
                return Color.yellow;

            case LedColor.White:
                return Color.white;

            default:
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    [RequireComponent(typeof(Renderer))]
    public class Led : MonoBehaviour
    {
        [SerializeField]
        private Renderer m_renderer;

        private LedColor m_color;

        void Start()
        {
            color = LedColor.Off;
        }

        public LedColor color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                if (m_renderer)
                {
                    if (value != LedColor.Off)
                    {
                        m_renderer.material.color = value.ToColor();
                        m_renderer.enabled = true;
                    }
                    else
                    {
                        m_renderer.enabled = false;
                    }
                }
            }
        }

        void Reset()
        {
            m_renderer = GetComponent<Renderer>();
        }
    }
}
