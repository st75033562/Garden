using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSettings : MonoBehaviour
{
    public ScrollRect.MovementType movementType;

    void Awake()
    {
#if UNITY_STANDALONE_WIN
        var scrollRect = GetComponent<ScrollRect>();
        if (scrollRect)
        {
            scrollRect.movementType = movementType;
        }
#endif
    }
}
