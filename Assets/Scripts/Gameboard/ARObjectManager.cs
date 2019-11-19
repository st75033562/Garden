using UnityEngine;

namespace Gameboard
{
    public class ARObjectManager : ARSceneManager
    {
        public ObjectManager objectManager
        {
            get;
            set;
        }

        public override int GetMarkerObjectId(int markerId)
        {
            var obj = GetMarkerObject(markerId);
            if (obj)
            {
                return obj.GetComponent<Entity>().id;
            }
            return 0;
        }

        protected override void OnObjectCreated(GameObject go)
        {
            objectManager.Register(go.GetComponent<Entity>(), true);
        }
    }
}
