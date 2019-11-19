using System;
using UnityEngine;

namespace Gameboard
{
    /// <summary>
    /// shared object action config.
    /// </summary>
    public class ObjectActionConfig : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            /// <summary>
            /// full name of ObjectAction implementation class
            /// </summary>
            public string className;
            public string jsonConfig;
        }

        public Entry[] m_entries;
    }
}
