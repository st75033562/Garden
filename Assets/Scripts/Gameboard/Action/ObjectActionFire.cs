using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    [ObjectActionParameter(1)]
    public class ObjectActionFire : ObjectAction
    {
        public GameObject m_ammoTemplate;
        public Transform m_muzzle;

        private readonly List<GameObject> m_objects = new List<GameObject>();

        public override void Execute(object o, params string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            float distance;
            if (float.TryParse(args[0], out distance))
            {
                var entity = GetComponent<Entity>();
                var instance = Instantiate(m_ammoTemplate, entity.sceneRoot, false);
                instance.transform.position = m_muzzle.position;
                instance.transform.forward = m_muzzle.forward;

                var bullet = instance.GetComponent<Bullet>();
                bullet.SetFlyDis(distance);
                bullet.spawningEntity = GetComponent<Entity>();

                m_objects.Add(instance);
            }
        }

        public override void Stop()
        {
            foreach (var obj in m_objects)
            {
                if (obj)
                {
                    Destroy(obj);
                }
            }
            m_objects.Clear();
        }
    }
}
