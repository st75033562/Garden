using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public interface IPopupSelectAttachmentDelegate
{
    void OnConfirm(Action uploadAttachment);

    void OnUploadFinished(IEnumerable<LocalResData> attachments);
}

public class PopupSelectAttachment : PopupController
{
    public class Payload
    {
        public IEnumerable<LocalResData> resources;
        public IPopupSelectAttachmentDelegate eventDelegate;
        public Color? themeColor;
        public bool editable = true;
    }

    public Color m_themeColor;
    public GameObject m_progressGo;
    public ProgressBar m_progressBar;

    public Graphic[] m_replaceColorGraphics;

    public GameObject m_scrollUI;
    public ScrollableAreaController m_attachmentScrollArea;
    public SelectResType m_selectResUI;
    public GameObject m_emptyGo;
    public GameObject m_emptyAddButton;
    public GameObject m_emptyText;

    private IPopupSelectAttachmentDelegate m_eventDelegate;
    private readonly List<LocalResData> m_attachments = new List<LocalResData>();
    private List<LocalResData> m_pendingVideos = new List<LocalResData>();
    private int m_uploadCount;
    private int m_uploadFinishedCount;
    private readonly Dictionary<HttpRequest, float> m_progresses = new Dictionary<HttpRequest, float>();
    private readonly List<HttpRequest> m_tasks = new List<HttpRequest>();
    private bool m_editable;

    protected override void Start()
    {
        base.Start();

        m_progressGo.SetActive(false);

        var arg = (Payload)payload;
        m_editable = arg.editable;
        m_eventDelegate = arg.eventDelegate;
        m_progressBar.hint = "ui_uploading_attachments".Localize();
        if (arg.themeColor != null)
        {
            UpdateColor(arg.themeColor.Value);
        }

        m_selectResUI.ListenResData(OnSelectResource);

        // for `+' button
        if (m_editable)
        {
            m_attachments.Add(null);
        }
        if (arg.resources != null)
        {
            m_attachments.AddRange(arg.resources);
        }
        UpdateUI();
    }

    private void UpdateColor(Color newColor)
    {
        foreach (var g in m_replaceColorGraphics)
        {
            g.color = newColor;
        }
    }

    private void OnSelectResource(LocalResData resource)
    {
        m_attachments.Add(resource);
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool empty = m_attachments.Count == (m_editable ? 1 : 0);
        m_emptyGo.SetActive(empty);
        m_emptyText.SetActive(empty && !m_editable);
        m_emptyAddButton.SetActive(empty && m_editable);
        m_scrollUI.SetActive(!empty);

        m_attachmentScrollArea.InitializeWithData(
            m_attachments.Select(x => new AttachmentCellData {
                resData = x,
                deletable = m_editable,
            }).ToList()
        );
    }

    public void OnClickConfirm()
    {
        m_eventDelegate.OnConfirm(UploadAttachments);
    }

    private void UploadAttachments()
    {
        m_uploadCount = 0;
        m_uploadFinishedCount = 0;
        m_pendingVideos.Clear();

        m_progressGo.SetActive(true);
        UpdateProgress(0.0f);

        foreach (LocalResData res in m_attachments)
        {
            if (res == null ||
                Utils.IsValidUrl(res.name) ||
                (res.resType == ResType.Video && res.filePath == null) ||
                (res.resType == ResType.Image && res.textureData == null) ||
                (res.resType == ResType.Course && res.textureData == null))
            {
                continue;
            }

            m_uploadCount++;
            if (res.resType == ResType.Video)
            {
                m_pendingVideos.Add(res);
            }
            else
            {
                var request = Uploads.UploadMedia(res.textureData, res.name, false);
                request.finalHandler = () => { UploadResComplete(request); };
                request.uploadProgressHandler = GetProgressHandler(request);
                request.Execute();
            }
        }
        UploadVideo();

        if (m_uploadCount == 0)
        {
            UploadResComplete(null);
        }
    }

    void UploadVideo()
    {
        if (m_pendingVideos.Count > 0)
        {
            LocalResData res = m_pendingVideos[0];
            m_pendingVideos.RemoveAt(0);

            LoadResource.instance.LoadLocalRes(res.filePath, (www) => {

                var request = Uploads.UploadMedia(www.bytes, res.name, true);
                request.successHandler = delegate {
                    UploadResComplete(request);
                    UploadVideo();
                };
                request.errorHandler = () => UploadResComplete(request);
                request.uploadProgressHandler = GetProgressHandler(request);
                request.Execute();
                m_tasks.Add(request);
            });
        }
        else
        {
            GalleryUtils.RemoveTempFiles();
        }
    }

    private Action<float> GetProgressHandler(HttpRequest request)
    {
        return progress => {
            m_progresses[request] = progress;
            UpdateProgress(m_progresses.Values.Sum() / m_uploadCount);
        };
    }

    private void UpdateProgress(float progress)
    {
        m_progressBar.progress = progress;
    }

    private void UploadResComplete(HttpRequest request)
    {
        m_tasks.Remove(request);
        ++m_uploadFinishedCount;
        if (m_uploadFinishedCount >= m_uploadCount)
        {
            m_progressGo.SetActive(false);
            m_progresses.Clear();
            m_eventDelegate.OnUploadFinished(m_attachments.Skip(m_editable ? 1 : 0));
            OnCloseButton();
        }
    }

    public void OnClickAddAttachment()
    {
        m_selectResUI.gameObject.SetActive(true);
    }

    public void OnClickRemoveAttachment(AttachmentCell cell)
    {
        var res = cell.DataObject as AttachmentCellData;
        m_attachments.Remove(res.resData);
        UpdateUI();
    }

    public override void OnCloseButton()
    {
        foreach (var request in m_tasks)
        {
            request.Abort();
        }
        m_tasks.Clear();
        base.OnCloseButton();
    }
}
