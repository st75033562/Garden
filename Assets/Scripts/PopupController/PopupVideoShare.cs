using Google.Protobuf;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class SharedVideo
{
    public uint id;
    public uint userId;
    public string username;
    public SharedVideoInfo info;

    private Uri m_path;

    public SharedVideo() {}

    public SharedVideo(uint id, uint userId, string username, SharedVideoInfo info)
    {
        this.id = id;
        this.userId = userId;
        this.username = username;
        this.info = info;
    }

    public bool isMine
    {
        get { return userId == UserManager.Instance.UserId; }
    }

    public bool isShare 
    {
        get { return userId != 0; }
    }

    public Uri path
    {
        get
        {
            if (m_path == null)
            {
                if (isLocal)
                {
                    m_path = new Uri(info.filename);
                }
                else
                {
                    m_path = new Uri(g_WebRequestManager.instance.GetMediaPath(info.filename, true));
                }
            }
            return m_path;
        }
    }

    public bool isLocal
    {
        get { return id == 0; }
    }
    
    public static SharedVideo LocalFile(string filepath)
    {
        return new SharedVideo {
            userId = UserManager.Instance.UserId,
            username = UserManager.Instance.Nickname,
            info = new SharedVideoInfo {
                filename = filepath,
            }
        };
    }
}

public class SharedVideoInfo
{
    // name of the video, for local video, this is the full path
    public string filename;
    public string title;
}

public class PopupVideoShare : PopupController
{
    public UIButtonToggle m_deleteModeButton;
    public ScrollableAreaController[] m_scrollControllers;

    private enum ViewType
    {
        All,
        Mine,
        Local,
        Count
    }

    private ViewType m_currentViewType = ViewType.All;

    private class ViewState
    {
        public List<SharedVideo> videos = new List<SharedVideo>();
        public bool dirty = true;
    }

    private ViewState[] m_viewStates = new ViewState[(int)ViewType.Count];

    protected override void Start()
    {
        base.Start();

        EventBus.Default.AddListener(EventId.LocalVideoShared, OnVideoShared);
        OnClickAllVideos();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventBus.Default.RemoveListener(EventId.LocalVideoShared, OnVideoShared);
    }

    private void OnVideoShared(object userData)
    {
        var eventData = (LocalVideoSharedEventData)userData;

        var sharedVideo = new SharedVideo {
            id = eventData.shareId,
            userId = UserManager.Instance.UserId,
            username = UserManager.Instance.Nickname,
            info = eventData.info
        };

        AddSharedVideo(sharedVideo, ViewType.All);
        AddSharedVideo(sharedVideo, ViewType.Mine);
    }

    private void AddSharedVideo(SharedVideo video, ViewType type)
    {
        var state = m_viewStates[(int)type];
        if (state != null && state.videos.FindIndex(x => x.id == video.id) == -1)
        {
            state.videos.Add(video);
            state.dirty = true;
            if (type == m_currentViewType)
            {
                Refresh();
            }
        }
    }

    public void OnClickAllVideos()
    {
        SetCurrentViewType(ViewType.All);
        if (m_viewStates[(int)ViewType.All] == null)
        {
            GetVideos(GetShared_Type.GetAll, () => Refresh());
        }
    }

    public void OnClickMyVideos()
    {
        SetCurrentViewType(ViewType.Mine);
        if (m_viewStates[(int)ViewType.Mine] == null)
        {
            GetVideos(GetShared_Type.GetSelf, () => Refresh());
        }
    }

    private void GetVideos(GetShared_Type type, Action done)
    {
        int popupId = PopupManager.ShowMask();

        var args = new CMD_Get_Shared_List_r_Parameters();
        args.ReqType = type;

        SocketManager.instance.send(Command_ID.CmdGetSharedListR, args.ToByteString(), (code, result) => {
            PopupManager.Close(popupId);

            if (code == Command_Result.CmdNoError)
            {
                var viewType = type == GetShared_Type.GetAll ? ViewType.All : ViewType.Mine;
                var state = m_viewStates[(int)viewType] = new ViewState();

                var response = CMD_Get_Shared_List_a_Parameters.Parser.ParseFrom(result);
                state.videos.AddRange(response.SharedList.Select(x => {
                    var info = JsonMapper.ToObject<SharedVideoInfo>(x.SharedOpts);
                    return new SharedVideo(x.SharedId, x.CreateId, x.CreateNickname, info);
                }));

                done();
            }
            else
            {
                PopupManager.Notice(code.Localize());
            }
        });
    }

    public void OnClickLocalVideos()
    {
        var state = m_viewStates[(int)ViewType.Local];
        if (state == null)
        {
            state = new ViewState();
            state.videos.AddRange(VideoRecorder.GetVideoPaths().Select(x => SharedVideo.LocalFile(x)));
            m_viewStates[(int)ViewType.Local] = state;
        }

        SetCurrentViewType(ViewType.Local);
    }

    public bool isInDeleteMode
    {
        get { return m_deleteModeButton.isOn; }
    }

    private void SetCurrentViewType(ViewType viewType)
    {
        // cancel delete mode first
        m_deleteModeButton.isOn = false;

        m_currentViewType = viewType;
        for (int i = 0; i < (int)ViewType.Count; ++i)
        {
            m_scrollControllers[i].gameObject.SetActive(i == (int)viewType);
        }

        Refresh();
    }

    public void OnClickVideoCell(VideoCell cell)
    {
        if (isInDeleteMode)
        {
            DeleteVideo(cell);
        }
        else
        {
            PopupManager.VideoPreview(cell.data, RemoveVideoData);
        }
    }

    private void DeleteVideo(VideoCell cell)
    {
        if (!cell.data.isLocal)
        {
            var args = new CMD_Del_Shared_r_Parameters();
            args.SharedId = cell.data.id;

            int popupId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelSharedR, args.ToByteString(), (code, result) => {
                PopupManager.Close(popupId);

                if (code == Command_Result.CmdNoError)
                {
                    RemoveVideoData(cell.data);
                }
                else
                {
                    PopupManager.Notice("ui_video_delete_failed".Localize());
                }
            });
        }
        else
        {
            try
            {
                File.Delete(cell.data.path.LocalPath);
                RemoveVideoData(cell.data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PopupManager.Notice("ui_video_delete_failed".Localize());
            }
        }
    }

    private void Refresh(bool forced = false)
    {
        var state = m_viewStates[(int)m_currentViewType];
        if (state != null && (state.dirty || forced))
        {
            state.dirty = false;
            m_scrollControllers[(int)m_currentViewType].InitializeWithData(state.videos);
        }

        m_deleteModeButton.interactable = state != null &&
                                          state.videos.Count > 0 &&
                                          m_currentViewType != ViewType.All;
    }

    private void RemoveVideoData(SharedVideo video)
    {
        if (video.isLocal)
        {
            RemoveVideoData(video, ViewType.Local);
        }
        else
        {
            RemoveVideoData(video, ViewType.All);
            if (video.isMine)
            {
                RemoveVideoData(video, ViewType.Mine);
            }
        }

        // exit delete mode
        var currentState = m_viewStates[(int)m_currentViewType];
        if (isInDeleteMode && currentState.videos.Count == 0)
        {
            m_deleteModeButton.isOn = false;
            currentState.dirty = true;
        }

        Refresh();
    }

    private void RemoveVideoData(SharedVideo video, ViewType type)
    {
        var state = m_viewStates[(int)type];
        int index = state.videos.IndexOf(video);
        if (index != -1)
        {
            state.videos.RemoveAt(index);
            state.dirty = true;
        }
    }

    public void OnDeleteModeChanged()
    {
        Refresh(true);
    }
}
