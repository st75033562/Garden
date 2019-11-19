using UnityEngine;

public class StateObject : MonoBehaviour
{
    public GameObject stateOn;
    public GameObject stateOff;

    [SerializeField]
    [HideInInspector]
    private bool m_stateOn;

    private void Awake()
    {
        on = m_stateOn;
    }

    public bool on
    {
        get { return m_stateOn; }
        set
        {
            m_stateOn = value;
            if (stateOn)
            {
                stateOn.SetActive(value);
            }
            if (stateOff)
            {
                stateOff.SetActive(!value);
            }
        }
    }
}
