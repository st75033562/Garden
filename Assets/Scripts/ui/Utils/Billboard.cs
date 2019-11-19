using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera m_targetCamera;

    void Start()
    {
        m_targetCamera = Utils.FindCamera(gameObject.layer);
    }

    void LateUpdate()
    {
        if (m_targetCamera)
        {
            transform.rotation = Quaternion.LookRotation(m_targetCamera.transform.forward, m_targetCamera.transform.up);
        }
    }
}
