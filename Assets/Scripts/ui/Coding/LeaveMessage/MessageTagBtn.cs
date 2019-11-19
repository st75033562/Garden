using UnityEngine;

public class MessageTagBtn : MonoBehaviour
{
	public MessageTag m_Tag;
	public GameObject m_CloseImage;
	public GameObject m_OpenImage;

    private bool m_IsOpen;

    void Awake()
    {
        RefreshUI();
    }

	void RefreshUI()
	{
		m_CloseImage.SetActive(!IsOpen);
		m_OpenImage.SetActive(IsOpen);
	}

	public void ClickTag()
	{
        m_Tag.OnClickTagBtn(this);
    }

    public string Key
    {
        get;
        set;
    }

    public bool IsOpen
    {
        get { return m_IsOpen; }
        set
        {
            m_IsOpen = value;
            RefreshUI();
        }
    }
}
