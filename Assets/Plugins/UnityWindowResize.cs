using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_WIN
public class UnityWindowResize : MonoBehaviour
{
    public float aspect;
    public int minWidth;

    [DllImport("unity_window_resize", CallingConvention = CallingConvention.Cdecl)]
    private static extern int uwr_init(float aspect, int min_width);

    [DllImport("unity_window_resize", CallingConvention = CallingConvention.Cdecl)]
    private static extern void uwr_uninit();

    void Start()
    {
        if (aspect > 0)
        {
            uwr_init(aspect, minWidth);
        }
        else
        {
            Debug.LogError("aspect must be positive");
        }
    }

    void OnApplicationQuit()
    {
        uwr_uninit();
    }
}
#endif