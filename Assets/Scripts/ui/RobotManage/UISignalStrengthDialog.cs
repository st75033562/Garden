using Robomation;
using Robomation.BLE;
using UnityEngine;
using UnityEngine.UI;

public class UISignalStrengthDialog : MonoBehaviour
{
    public Text textStrength;
    public Slider rssiSlider;

    private IBLEConnection m_bleConnection;

    void Start()
    {
        m_bleConnection = RobotManager.instance.connection as IBLEConnection;
        if (m_bleConnection != null)
        {
            rssiSlider.value = m_bleConnection.minRSSI;
        }
    }

    void OnEnable()
    {
        if (m_bleConnection != null)
        {
            OnStrengthChanged(m_bleConnection.minRSSI);
        }
    }

    public void OnStrengthChanged(float value)
    {
        var strength = (int)value;
        textStrength.text = strength.ToString();
        if(m_bleConnection != null)
            m_bleConnection.minRSSI = strength;
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}