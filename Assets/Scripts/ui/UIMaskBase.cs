using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

public class UIMaskBase : MonoBehaviour
{
	public Text m_Notice;
	int m_MaskCount = 0;

	public void ShowMask(string txt = "")
	{
		++m_MaskCount;
        m_Notice.text = txt;

		if(!gameObject.activeSelf)
		{
			gameObject.SetActive(true);
		}
	}

	public void CloseMask()
	{
        if (m_MaskCount > 0 && --m_MaskCount == 0)
		{
			gameObject.SetActive(false);
		}
        else if (m_MaskCount == 0)
        {
            Debug.LogWarning("close a already closed mask");
        }
	}

    public void ResetMask()
    {
        m_MaskCount = 0;
        gameObject.SetActive(false);
    }
}
