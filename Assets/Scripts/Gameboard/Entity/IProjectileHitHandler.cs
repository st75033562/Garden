using UnityEngine;

namespace Gameboard
{
    public interface IProjectileHitHandler
    {
        /// <summary>
        /// called when the projectile spawned by the entity hits the target object
        /// </summary>
        void OnHit(GameObject target);
    }
}
