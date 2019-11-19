using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public class VideoCell : ScrollableCell
{
    public Text m_titleText;
    public Text m_usernameText;

    public PopupVideoShare m_controller;

    public Image m_localVideoImage;
    public Image m_sharedVideoImage;
    public Image m_deleteVideoImage;

    private RaceLamp m_titleEffect;
    private RaceLamp m_usernameEffect;

    void Awake()
    {
        m_titleEffect = m_titleText.GetComponent<RaceLamp>();
        m_usernameEffect = m_usernameText.GetComponent<RaceLamp>();
    }

    public override void ConfigureCellData()
    {
        if (dataObject == null) { return; }

        if (m_controller.isInDeleteMode)
        {
            m_deleteVideoImage.enabled = true;
            m_localVideoImage.enabled = false;
            m_sharedVideoImage.enabled = false;
        }
        else
        {
            m_deleteVideoImage.enabled = false;
            m_localVideoImage.enabled = data.isLocal;
            m_sharedVideoImage.enabled = !data.isLocal;
        }

        if (!data.isLocal)
        {
            m_titleText.text = data.info.title;
            m_usernameText.text = data.username;
            m_usernameText.enabled = true;
        }
        else
        {
            m_titleText.text = Path.GetFileNameWithoutExtension(data.info.filename);
            m_usernameText.enabled = false;
        }

        m_titleEffect.ResetPostion();
        m_usernameEffect.ResetPostion();
    }

    public SharedVideo data
    {
        get { return (SharedVideo)dataObject; }
    }
}
