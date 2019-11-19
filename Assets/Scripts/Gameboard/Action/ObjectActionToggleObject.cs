using UnityEngine;

namespace Gameboard
{
    [ObjectActionParameter(-1)]
    public class ObjectActionToggleObject : ObjectAction
    {
        public GameObject[] m_objects;

        public int m_defaultActiveIndex = -1;
        public bool m_defaultOn = true;
        // true if toggle can be turn off, otherwise if the active toggle is turned off, 
        // the default group will be turned on.
        // NOTE: only used when m_isToggle is true.
        public bool m_allowToggleOff = true;

        private int m_activeIndex = -1;

        public class Config
        {
            [FieldValue(-1)]
            public int index = -1;
        }

        public void SetActive(int index, bool on)
        {
            if (index < -1 || index >= m_objects.Length)
            {
                return;
            }

            if (m_activeIndex == index)
            {
                if (m_activeIndex != -1)
                {
                    m_objects[m_activeIndex].SetActive(on || !m_allowToggleOff);
                }
                return;
            }
            else if (on)
            {
                if (m_activeIndex != -1)
                {
                    m_objects[m_activeIndex].SetActive(false);
                }
                m_activeIndex = index;

                if (index != -1)
                {
                    m_objects[index].SetActive(on);
                }
            }
        }

        public override void Execute(object o, params string[] args)
        {
            bool on = true;
            if (args.Length > 0)
            {
                int state;
                int.TryParse(args[0], out state);
                on = state != 0;
            }

            var config = (Config)o;
            SetActive(config.index, on);
        }

        public override void Stop()
        {
            SetActive(m_defaultActiveIndex, m_defaultOn);
        }
    }
}
