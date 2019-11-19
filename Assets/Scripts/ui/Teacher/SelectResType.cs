using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ResType
{
    Invalid,
    Image,
    Video,
    Course,
    Url
}

public class LocalResData
{
    public string name;
    public byte[] textureData;

    public ResType resType;

    public string filePath;

    public string nickName;

    public LocalResData() {}

    public LocalResData(string name, string nickName, ResType type)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name");
        }
        if (type == ResType.Invalid)
        {
            throw new ArgumentOutOfRangeException("type");
        }

        this.name = name;
        this.resType = type;
        this.nickName = nickName;
    }

    public LocalResData(LocalRes resource)
    {
        if (resource.type != LocalResType.IMAGE && resource.type != LocalResType.VIDEO && resource.type != LocalResType.COURSE)
        {
            throw new ArgumentException();
        }
        if(resource.type == LocalResType.COURSE) {
            name = resource.hash + ".pdf";
        } else {
            name = resource.hash;
        }
        textureData = resource.imageData;
        resType = (ResType)resource.type;
        filePath = resource.path;
    }

    public static LocalResData Image(string name, byte[] data)
    {
        return new LocalResData {
            name = name,
            textureData = data,
            resType = ResType.Image,
        };
    }

    public static LocalResData Image(byte[] data)
    {
        return Image(Md5.CreateMD5Hash(data), data);
    }

}

public class SelectResType : MonoBehaviour {
    [SerializeField]
    private GameObject panelType;
    [SerializeField]
    private GameObject panelSource;
    [SerializeField]
    private GameObject panelWebsite;
    [SerializeField]
    private GameObject localResButt;
    [SerializeField]
    private InputField inputFieldWebsite;

    private ResType selectType ;
    private List<ResType> selectTypeMark = new List<ResType>();

    private const int referenceHeight = 1080;

    private Action<LocalResData> selectResData;
    

    public void ListenResData(Action<LocalResData> action)
    {
        selectResData = action;
        
    }

    public void OnClickVideo()
    {
        gameObject.SetActive(true);
        SetSelectType(ResType.Video);
    }

    public void OnClickImage()
    {
        gameObject.SetActive(true);
        SetSelectType(ResType.Image);
    }

    public void OnClickCourse()
    {
        gameObject.SetActive(true);
        SetSelectType(ResType.Course);
    }

    public void OnClickLocal()
    {
        if(selectType == ResType.Video) {
            LocalResOperate.instance.OpenResWindow(LocalResType.VIDEO, (data) => {
                Close();
                selectResData(new LocalResData(data));
            });
        } else if(selectType == ResType.Image) {
            LocalResOperate.instance.OpenResWindow(LocalResType.IMAGE, (data) => {
                Texture2D texture = new Texture2D(0, 0);
                if(!texture.LoadImage(data.imageData)) {
                    Destroy(texture);
                    Debug.Log("failed to load image data " + data.path);
                    Close();
                    return;
                }

                if(texture.height > referenceHeight) {
                    TextureScale.Bilinear(texture, referenceHeight);
                }

                var res = LocalResData.Image(texture.EncodeToPNG());
                DestroyImmediate(texture);

                Close();
                selectResData(res);
            });
        } else if(selectType == ResType.Course) {
            LocalResOperate.instance.OpenResWindow(LocalResType.COURSE, (data) => {
                Close();
                selectResData(new LocalResData(data));
            });
        }
    }

    public void OnClickWebsite()
    {
        inputFieldWebsite.text = "";

        SetSelectType(ResType.Url);
    }

    public void OnClickClose()
    {
        if (selectTypeMark.Count > 1)
        {
            SetSelectType(selectTypeMark[selectTypeMark.Count - 2], false);
        }
        else if (selectTypeMark.Count > 0 && panelType != null)
        {
            ResetState();
            selectTypeMark.Clear();
        }
        else
        {
            Close();
        }
    }

    void ResetState()
    {
        if(panelType != null) { 
            panelType.SetActive(true);
        }
        panelSource.SetActive(false);
        panelWebsite.SetActive(false);
    }

    public void Close()
    {
        ResetState();
        gameObject.SetActive(false);
        selectTypeMark.Clear();
    }

    public void OnClickConfim()
    {
        try
        {
            var escapedUrl = Utils.UrlEncode(inputFieldWebsite.text);
            LocalResData data = new LocalResData();
            data.resType = selectType;
            data.name = escapedUrl;
            Close();
            selectResData(data);
        }
        catch (UriFormatException)
        {
            PopupManager.Notice("invalid_url".Localize());
        }
    }

    void SetSelectType(ResType selectType , bool addMark = true)
    {
        if(selectType != ResType.Url)
            this.selectType = selectType;
        switch (selectType)
        {
            case ResType.Video:
            case ResType.Image:
                LocalResMode();
                localResButt.SetActive(true);
                break;
            case ResType.Course:
                LocalResMode();
                localResButt.SetActive(!Application.isMobilePlatform);
                break; 
            case ResType.Url:
                panelWebsite.SetActive(true);
                panelSource.SetActive(false);
                if(panelType != null) {
                    panelType.SetActive(false);
                }
                break;
        }
        if (addMark)
            selectTypeMark.Add(selectType);
        else
            selectTypeMark.RemoveAt(selectTypeMark.Count - 1);
    }

    void LocalResMode() {
        panelSource.SetActive(true);
        if(panelType != null) {
            panelType.SetActive(false);
        }
        panelWebsite.SetActive(false);
    }
}
