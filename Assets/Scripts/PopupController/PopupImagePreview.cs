public class PopupImagePreview : PopupController
{
    public UIImageMedia m_imageMedia;

    public void SetImageData(byte[] data)
    {
        m_imageMedia.SetImage(data);
    }

    public void SetImageName(string name)
    {
        m_imageMedia.SetImage(name);
    }
}
