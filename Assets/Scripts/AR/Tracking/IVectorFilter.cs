using System;
using UnityEngine;

namespace AR
{
    interface IVector2Filter : IDisposable
    {
        Vector2 Filter(Vector2 v, float dt);
    }

    interface IVector3Filter : IDisposable
    {
        Vector3 Filter(Vector3 v, float dt);
    }
}
