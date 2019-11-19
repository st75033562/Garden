using UnityEngine;

public class CustomCurosr : MonoBehaviour
{
    // NOTE: the texture should be uncompressed
    public Texture2D texture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode mode = CursorMode.Auto;

    void OnEnable()
    {
        Cursor.SetCursor(texture, hotspot, mode);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
