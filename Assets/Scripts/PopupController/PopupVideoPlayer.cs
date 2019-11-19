public class PopupVideoPlayer : PopupController
{
    public UIVideoPlayer m_videoPlayer;

    protected override void Start()
    {
        base.Start();

        m_videoPlayer.SetUrl((string)payload);
        m_videoPlayer.Play();
    }

    protected override void DoClose()
    {
        base.DoClose();

        m_videoPlayer.Stop();
    }
}
