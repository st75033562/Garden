using UnityEngine;

namespace AR
{
    interface IQuaternionFilter
    {
        Quaternion Filter(Quaternion q);
    }
}
