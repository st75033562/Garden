using UnityEngine;

namespace Gameboard
{
    public abstract class LightSource : MonoBehaviour, ILightSource, IVisible
    {
        public abstract float flux
        {
            get;
            set;
        }

        public abstract float ComputeIlluminance(Vector3 position, Vector3 normal);

        public virtual void OnVisibilityChanged(bool visible) { }
    }
}
