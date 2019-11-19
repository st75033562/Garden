using System;
using System.IO;
using UnityEngine;
using Google.Protobuf;

public class PopupVideoPreviewPayload
{
    public Action<SharedVideo> onVideoDeleted;
    public SharedVideo video;
    public bool ShowCloseBtn;
}

public class PopupVideoPreview : PopupController
{
    public UIVideoPlayer m_videoPlayer;
    public GameObject m_deleteButton;
    public GameObject m_shareButton;

    private PopupVideoPreviewPayload m_payload;

    protected override void Start()
    {
        base.Start();

        m_payload = base.payload as PopupVideoPreviewPayload;

        if (m_payload == null)
        {
            m_payload = new PopupVideoPreviewPayload {
                video = SharedVideo.LocalFile("")
            };
        }

        m_videoPlayer.SetUrl(m_payload.video.path.ToString());
        m_videoPlayer.Play();
        UpdateUI();
    }

    private void UpdateUI()
    {
        m_deleteButton.SetActive(m_payload.video.isMine);
        m_shareButton.SetActive(m_payload.video.isShare);
    }

    public void OnClickDelete()
    {
        if (m_payload.video.isLocal)
        {
            m_videoPlayer.Abort();
            try
            {
                File.Delete(m_payload.video.path.LocalPath);
                if (m_payload.onVideoDeleted != null)
                {
                    m_payload.onVideoDeleted(m_payload.video);
                }
                Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PopupManager.Notice("ui_video_delete_failed".Localize());
            }
        }
        else
        {
            var args = new CMD_Del_Shared_r_Parameters();
            args.SharedId = m_payload.video.id;

            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelSharedR, args.ToByteString(), (code, result) => {
                if (code == Command_Result.CmdNoError)
                {
                    if (m_payload.onVideoDeleted != null)
                    {
                        m_payload.onVideoDeleted(m_payload.video);
                    }
                    Close();
                }
                else
                {
                    PopupManager.Notice("ui_video_delete_failed".Localize());
                }
                PopupManager.Close(popupId);
            });
            
        }
    }

    public void OnClickShare()
    {
        PopupManager.ShareVideoPlatform(m_payload.video);
    }

    protected override void DoClose()
    {
        base.DoClose();

        m_videoPlayer.Stop();
    }
}
