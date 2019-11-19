using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float m_duration;

    void Start()
    {
        DestroyObject(gameObject, m_duration);
    }
}
