using UnityEngine;

namespace Gameboard
{
    [ObjectActionName("ObjectAction")]
    public abstract class ObjectAction : MonoBehaviour
    {
        public abstract void Execute(object o, params string[] args);

        public virtual void Stop() { }
    }
}
