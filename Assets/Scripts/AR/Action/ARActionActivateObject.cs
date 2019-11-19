using Gameboard;
using System;
using System.Collections;
using UnityEngine;

namespace AR
{
    [ObjectActionParameter(-1)]
    public class ARActionActivateObject : ARAction
    {
        [Serializable]
        public class Group
        {
            public GameObject[] m_Obj;
        }

        public Group[] m_Action;

        public class Config
        {
            [FieldValue(-1)]
            public int index = -1;
        }

        public override void Execute(object o, params string[] userArgs)
        {
            Config config = (Config)o;
            if (config.index < 0 || config.index >= m_Action.Length)
            {
                UnityEngine.Debug.LogError("invalid index: " + config.index);
                return;
            }

            for (int i = 0; i < userArgs.Length; ++i)
            {
                float flag;
                if (float.TryParse(userArgs[i], out flag))
                {
                    var objects = m_Action[config.index].m_Obj;
                    if (i < objects.Length)
                    {
                        objects[i].SetActive(flag > 0);
                    }
                }
            }
        }

        public override void Stop()
        {
            for (int i = 0; i < m_Action.Length; ++i)
            {
                for (int j = 0; j < m_Action[i].m_Obj.Length; ++j)
                {
                    m_Action[i].m_Obj[j].SetActive(false);
                }
            }
        }
    }
}
