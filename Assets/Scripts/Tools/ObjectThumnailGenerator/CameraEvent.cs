using UnityEngine;
using System;

public class CameraEvent : MonoBehaviour
{
    public event Action onPostRender;

    void OnPostRender()
    {
        if (onPostRender != null)
        {
            onPostRender();
        }
    }
}
