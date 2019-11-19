using UnityEngine;
using RobotSimulation;

namespace Gameboard
{
    [ObjectActionParameter(1)]
    public class ObjectActionRotate : ObjectAction
    {
        public Transform m_target;

        private float m_identityRotation;

        void Awake()
        {
            m_identityRotation = m_target.eulerAngles.y;
        }

        public override void Execute(object o, params string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            float angle;
            if (float.TryParse(args[0], out angle))
            {
                m_target.eulerAngles = new Vector3(0, m_identityRotation - angle, 0);
            }
        }

        public override void Stop()
        {
            m_target.localEulerAngles = Vector3.zero;
        }
    }
}
