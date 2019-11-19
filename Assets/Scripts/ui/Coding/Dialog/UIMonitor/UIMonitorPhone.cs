using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMonitorPhone : MonoBehaviour {
	public Text m_X_Acceleration;
	public Text m_Y_Acceleration;
	public Text m_Z_Acceleration;
	public Text m_X_Angular;
	public Text m_Y_Angular;
	public Text m_Z_Angular;

    public float m_Interval;

	void OnEnable () {
        StartCoroutine(UpdateUI());
	}

    IEnumerator UpdateUI()
    {
        for (; ;)
        {
    		Vector3 mAcc = Input.acceleration * 9.81f;
            m_X_Acceleration.text = mAcc.x.ToString("F6");
    		m_Y_Acceleration.text = mAcc.y.ToString("F6");
    		m_Z_Acceleration.text = mAcc.z.ToString("F6");
    		m_X_Angular.text = Input.gyro.rotationRate.x.ToString("F6");
    		m_Y_Angular.text = Input.gyro.rotationRate.y.ToString("F6");
    		m_Z_Angular.text = Input.gyro.rotationRate.z.ToString("F6");

            if (m_Interval > 0)
            {
                yield return new WaitForSeconds(m_Interval);
            }
        }
    }
	
}
