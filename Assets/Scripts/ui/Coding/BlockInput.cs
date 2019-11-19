using UnityEngine;

public class BlockInput : MonoBehaviour
{
    public bool GetMouseButtonDown(int button)
    {
        return enabled && Input.GetMouseButtonDown(button);
    }

    public bool GetMouseButton(int button)
    {
        return enabled && Input.GetMouseButton(button);
    }
}
