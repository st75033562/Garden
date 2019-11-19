using UnityEngine;

namespace Gameboard
{
    public abstract class Positional : MonoBehaviour
    {
        protected virtual void Awake()
        {
            entity = GetComponent<Entity>();
        }

        public Entity entity { get; private set; }

        /// <summary>
        /// set the position of the object, coordinate system is object dependent
        /// </summary>
        public abstract Vector3 position { get; set; }

        public abstract Vector3 localScale { get; set; }

        // depending on the positional type, rotations on certain axis may be ignored
        public abstract Vector3 rotation { get; set; }

        /// <summary>
        /// synchronize the internal state with the Transform
        /// </summary>
        public virtual void Synchornize() { }
    }
}
