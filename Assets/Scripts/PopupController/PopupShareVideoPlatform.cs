using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using Google.Protobuf;
using cn.sharesdk.unity3d;
using ZXing;
using ZXing.Common;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class LocalVideoSharedEventData
{
    public uint shareId;
    public SharedVideoInfo info;
}

public class PopupShareVideoPlatform : PopupController
{
    public GameObject m_appServerButton;
    public InputField m_titleInput;
    public GameObject m_wechatMomentsButton;
    public Text m_wechatText;

    public GameObject m_uploadingUI;
    public ProgressBar m_uploadProgressBar;
    public GameObject m_barcodeGo;
    public RawImage m_barcodeRawImage;

    private SharedVideo m_video;
    private string m_videoName;

    private ShareSDK m_shareSdk;

    private Texture2D m_barcodeTexture;

    protected override void Start()
    {
        base.Start();

        m_uploadingUI.SetActive(false);

        m_video = payload as SharedVideo;
        // Disable temporarily
        // m_appServerButton.SetActive(m_video.isLocal);
        m_appServerButton.SetActive(false);

        if (Application.isMobilePlatform)
        {
            m_wechatText.text = "ui_video_share_wechat_friends".Localize();
            m_shareSdk = FindObjectOfType<ShareSDK>();
            m_shareSdk.shareHandler += OnShareComplete;
        }
        else
        {
            m_wechatText.text = "ui_video_share_wechat".Localize();
        }

        m_wechatMomentsButton.SetActive(Application.isMobilePlatform);
        m_barcodeGo.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_shareSdk != null)
        {
            m_shareSdk.shareHandler -= OnShareComplete;
        }

        if (m_barcodeTexture)
        {
            Destroy(m_barcodeTexture);
        }
    }

    public void OnClickAppServer()
    {
        if (!CheckTitle()) { return; }

        UploadVideo(ShareVideoToAppServer);
    }

    private bool CheckTitle()
    {
        if (m_titleInput.text.Length == 0)
        {
            PopupManager.Notice("ui_video_empty_title_warning".Localize());
            return false;
        }
        return true;
    }

    private void UploadVideo(Action done)
    {
        if (m_videoName != null || !m_video.isLocal)
        {
            done();
            return;
        }

        byte[] content;
        try
        {
            content = File.ReadAllBytes(m_video.path.LocalPath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            PopupManager.Notice("ui_video_read_video_failed".Localize());
            return;
        }

        var videoName = Md5.CreateMD5Hash(content);

        m_uploadingUI.SetActive(true);

        var request = Uploads.UploadMedia(content, videoName, true);
        request.defaultErrorHandling = false;
        request.Success(() => {
                   m_uploadingUI.SetActive(false);
                   m_videoName = videoName;
                   done();
               })
               .Error(() => {
                   PopupManager.Notice("ui_video_share_video_failed".Localize());
                   m_uploadingUI.SetActive(false);
               })
               .UploadProgress(progress => {
                   m_uploadProgressBar.progress = progress;
               })
               .Execute();
        m_uploadProgressBar.progress = 0;
    }

    private void ShareVideoToAppServer()
    {
        var args = new CMD_Add_Shared_r_Parameters();
        var videoInfo = new SharedVideoInfo {
            filename = m_videoName,
            title = m_titleInput.text
        };
        args.SharedOpts = JsonMapper.ToJson(videoInfo);
        SocketManager.instance.send(Command_ID.CmdAddSharedR, args.ToByteString(), (code, result) => {
            m_uploadingUI.SetActive(false);
            if (code == Command_Result.CmdNoError)
            {
                var response = CMD_Add_Shared_a_Parameters.Parser.ParseFrom(result);
                EventBus.Default.AddEvent(EventId.LocalVideoShared,
                    new LocalVideoSharedEventData {
                        shareId = response.SharedId,
                        info = videoInfo
                    });

                PopupManager.Notice("ui_video_share_success".Localize());
            }
            else
            {
                PopupManager.Notice(code.Localize());
            }
        });
    }

    public void OnClickWechat()
    {
        if (!CheckTitle()) { return; }

        UploadVideo(() => ShareVideoToWechat(PlatformType.WeChat));
    }

    public void OnClickWechatMoments()
    {
        if (!CheckTitle()) { return; }

        UploadVideo(() => ShareVideoToWechat(PlatformType.WeChatMoments));
    }

    public void HideBarCode()
    {
        m_barcodeGo.SetActive(false);
    }

    private void ShareVideoToWechat(PlatformType platform)
    {
        if (!Application.isMobilePlatform)
        {
            if (!m_barcodeTexture)
            {
                var size = new Vector2(256, 256);
                m_barcodeTexture = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGBA32, false);
                // show QR code
                var writer = new BarcodeWriter() {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions {
                        Width = (int)size.x,
                        Height = (int)size.y
                    }
                };
                var colors = writer.Write(GetVideoShareUrl());
                m_barcodeTexture.SetPixels32(colors);
                m_barcodeTexture.Apply();
                m_barcodeRawImage.texture = m_barcodeTexture;
            }
            m_barcodeGo.SetActive(true);
            return;
        }

        var content = new ShareContent();
        content.SetShareType(ContentType.Video);
        content.SetUrl(GetVideoShareUrl());

        string thumbPath = null;
        if (m_video.isLocal)
        {
            thumbPath = VideoThumbnailCache.GetThumbnailPath(m_video.path.LocalPath);
        }

        content.SetImagePath(thumbPath ?? VideoThumbnailCache.defaultThumbnailPath);
        content.SetTitle(m_titleInput.text);
        content.SetText("我分享了视频，快来看看吧！");
        m_shareSdk.ShareContent(platform, content);
    }

    private string GetVideoShareUrl()
    {
        string videoName = m_videoName;
        if (!m_video.isLocal)
        {
            videoName = Path.GetFileNameWithoutExtension(m_video.path.AbsolutePath);
        }
        return g_WebRequestManager.instance.GetVideoShareUrl(videoName);
    }

    private void OnShareComplete(int reqID, ResponseState state, PlatformType type, System.Collections.Hashtable data)
    {
        if (state != ResponseState.Success)
        {
            Debug.LogError(data.toJson());
        }
    }
}
