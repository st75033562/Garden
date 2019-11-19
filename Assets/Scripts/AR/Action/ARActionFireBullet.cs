using System;
using System.Collections;
using UnityEngine;

namespace AR
{
    public class ARActionFireBullet : ARAction
    {
        [Serializable]
        public class BulletConfig
        {
            public GameObject prefab;
            public Transform[] anchors;
        }

        public class Config
        {
            public string name;

            [FieldValue(false)]
            public bool inherit_scale = true;
        }

        public BulletConfig[] bulletConfigs;
        

        public override void Execute(object o, params string[] userArgs)
        {
            Config config = (Config)o;
            if (config.name == null)
            {
                UnityEngine.Debug.LogError("invalid prefab name");
                return;
            }

            var bConfig = Array.Find(bulletConfigs, x => x.prefab.name == config.name);
            if (bConfig == null)
            {
                UnityEngine.Debug.LogError("invalid prefab name: " + config.name);
                return;
            }

            for (int i = 0; i < userArgs.Length; ++i)
            {
                if (i < bConfig.anchors.Length)
                {
                    float dist;
                    if (float.TryParse(userArgs[i], out dist) && dist > 0)
                    {
                        GameObject tPaodan = Instantiate(bConfig.prefab, bConfig.anchors[i].position, bConfig.anchors[i].rotation, SceneManager.SceneRoot);
                        if (config.inherit_scale)
                        {
                            tPaodan.transform.localScale = bConfig.anchors[i].lossyScale;
                        }
                        Bullet tBullet = tPaodan.GetComponent<Bullet>();
                        Vector3 vDir = (ParentObj).transform.forward;
                        Vector2 vDir2 = new Vector2(vDir.x, vDir.z);
                        vDir2 = vDir2.normalized;
                        tBullet.transform.forward = vDir.normalized;

                        tBullet.SetFlyDis(dist * SceneManager.WorldUnitPerCm);
                        
                        
                        //tBullet.transform.forward = new Vector3(vDir2.x, 0, vDir2.y);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}