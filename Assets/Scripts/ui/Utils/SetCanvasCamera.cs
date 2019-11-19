using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SetCanvasCamera : MonoBehaviour
{
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Utils.FindCamera(gameObject.layer);
    }
}