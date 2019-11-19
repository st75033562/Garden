using Google.Protobuf;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CertificateSetting {
    public void ParseJson(string data)
    {
        JsonUtility.FromJsonOverwrite(data, this);
    }

    public int certificateId;
    public string courseName;
}

public class TrophySetting {
    public static TrophySetting Parse(Trophy_Setting trophySetting)
    {
        var tp = JsonUtility.FromJson<TrophySetting>(trophySetting.TrophyJsonSetting);
        tp.miniScore = trophySetting.TrophyMinScore;
        return tp;
    }

    [NonSerialized]
    public int miniScore;

    public int bodyId;

    public int handleId;

    public int baseId;

    public int patternId;

    public int trophyResultId;

    public string courseName = "";

    public Trophy_Setting PackPb() {
        Trophy_Setting trophySetting = new Trophy_Setting();
        trophySetting.TrophyMinScore = miniScore;
        trophySetting.TrophyJsonSetting = JsonUtility.ToJson(this);

        return trophySetting;
    }

    public TrophySetting Clone() {
        TrophySetting clone = new TrophySetting();
        clone.miniScore = miniScore;
        clone.bodyId = bodyId;
        clone.handleId = handleId;
        clone.baseId = baseId;
        clone.patternId = patternId;
        clone.trophyResultId = trophyResultId;
        clone.courseName = courseName;
        return clone;
    }
}

public class UserCertificate : CertificateSetting {
    public static UserCertificate Parse(uint courseId, User_Certificate_Info info) {
        var cm = new UserCertificate() {
            courseId = courseId,
            awardTime = (long)info.AwardTime,
        };
        cm.ParseJson(info.CertificateSetting.CertificateJsonSetting);

        return cm;
    }
    public long awardTime { get; set; }
    public uint courseId { get; set; }
}

public class UserTrophy {
    public static UserTrophy Parse(uint courseId, User_Trophy_Info info) {
        UserTrophy cm = new UserTrophy() {
            courseId = courseId,
            awardTime = (long)info.AwardTime,
            awardTrophy = info.AwardTrophy,
            courseScore = info.CourseScore,
            courseRaceType = info.CourseRaceType,
            trophyPb = TrophySetting.Parse(info.TrophySetting)
        };
        return cm;
    }
    public TrophySetting trophyPb;
    public long awardTime { get; set; }
    public uint courseId { get; set; }
    public Trophy_Type awardTrophy { get; set; }
    public int courseScore { get; set; }
    public Course_Race_Type courseRaceType { get; set; }
}

public class HonorWallData {
    private static HonorWallData _instance = null;

    public static HonorWallData instance {
        get {
            if(_instance == null) {
                _instance = new HonorWallData();
            }
            return _instance;
        }
    }

    private List<UserCertificate> certificateInfos = new List<UserCertificate>();
    private List<UserTrophy> trophyInfos = new List<UserTrophy>();

    public List<UserCertificate> GetCertificates() {
        return certificateInfos;
    }

    public void AddCertificate(UserCertificate certificate) {
        if (certificateInfos.Find(x=> { return x.courseId == certificate.courseId; }) == null) {
            certificateInfos.Add(certificate);
        }
    }

    public List<UserTrophy> GetTrophys() {
        return trophyInfos;
    }

    public void AddTrophy(UserTrophy trophy) {
        if (trophyInfos.Find(x => { return x.courseId == trophy.courseId; }) == null)
        {
            trophyInfos.Add(trophy);
        }
    }

    public void Clear() {
        certificateInfos.Clear();
        trophyInfos.Clear();
    }

}

public static class HonourSortSettings
{
    public const string TrophyKeyName = "honour_trophy_sort";

    public enum SortType
    {
        Date,
        Name
    }

    public static readonly string[] SortOptions = new string[] {
        "ui_text_sort_date",
        "ui_text_sort_name"
    };

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)SortType.Date,
    };
}

public class PopupHonorConfigure
{
    public PopupHonour.Mode openType;
}

public class PopupHonour : PopupController {
    public Text honorCount;
    public ScrollLoopController scrollTrophy;
    public ScrollLoopController scrollCerificate;
    public Toggle toggleTrophy;
    public Toggle toggleCertificate;
    public UISortMenuWidget sortMenu;

    public enum Mode
    {
        Trophy,
        Certificate
    }

    private List<UserCertificate> certificateInfos ;
    private List<UserTrophy> trophyInfos;

    private Mode currentMode;
    private bool certificatDirty = true;
    private bool trophyDirty = true;

    private const int TrophiesPerRow = 4;
    private UISortSetting sortSetting;

    // Use this for initialization
    protected override void Start () {
        base.Start();
        certificateInfos = HonorWallData.instance.GetCertificates();
        trophyInfos = HonorWallData.instance.GetTrophys();

        sortSetting = (UISortSetting)UserManager.Instance.userSettings.Get(HonourSortSettings.TrophyKeyName, true);

        sortMenu.SetOptions(HonourSortSettings.SortOptions.Select(x => x.Localize()).ToArray());
        sortMenu.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        sortMenu.onSortChanged.AddListener(OnSortChanged);

        var config = (PopupHonorConfigure)payload;
        currentMode = config.openType;
        if(config.openType == Mode.Trophy) {
            toggleTrophy.isOn = true;
        } else {
            toggleCertificate.isOn = true;
        }

        honorCount.text = "ui_exist_trophy".Localize() + trophyInfos.Count;
        //  Refresh();

        if (currentMode == Mode.Trophy) {
            OnClickTrophy();
        }
        else
        {
            OnClickCertificate();
        }
    }

    private void OnSortChanged()
    {
        sortSetting.SetSortCriterion(sortMenu.activeSortOption, sortMenu.sortAsc);

        certificatDirty = true;
        trophyDirty = true;

        Refresh();
    }

    private void Refresh()
    {
        if (currentMode == Mode.Trophy && trophyDirty)
        {
            trophyDirty = false;
            scrollTrophy.initWithData(GetTrophyScrollData());
        }
        else if (currentMode == Mode.Certificate && certificatDirty)
        {
            certificatDirty = false;
            scrollCerificate.initWithData(GetCertificateScrollData());
        }
    }

    List<List<UserTrophy>> GetTrophyScrollData() {
        
        Comparison<UserTrophy> comparer = null;
        switch ((HonourSortSettings.SortType)sortSetting.sortKey)
        {
        case HonourSortSettings.SortType.Date:
            comparer = (x, y) => x.awardTime.CompareTo(y.awardTime);
            break;

        case HonourSortSettings.SortType.Name:
            comparer = (x, y) => x.trophyPb.courseName.CompareTo(y.trophyPb.courseName);
            break;
        }

        trophyInfos.Sort(comparer.Invert(!sortSetting.ascending));
        return trophyInfos.Split(TrophiesPerRow);
    }

    List<UserCertificate> GetCertificateScrollData()
    {
        Comparison<UserCertificate> comparer = null;
        switch ((HonourSortSettings.SortType)sortSetting.sortKey)
        {
        case HonourSortSettings.SortType.Date:
            comparer = (x, y) => x.awardTime.CompareTo(y.awardTime);
            break;

        case HonourSortSettings.SortType.Name:
            comparer = (x, y) => x.courseName.CompareTo(y.courseName);
            break;
        }

        certificateInfos.Sort(comparer.Invert(!sortSetting.ascending));
        return certificateInfos;
    }

    public void OnClickTrophy() {
        currentMode = Mode.Trophy;
        honorCount.text = "ui_exist_trophy".Localize() + trophyInfos.Count;
        scrollTrophy.gameObject.SetActive(true);
        scrollCerificate.gameObject.SetActive(false);

        Refresh();
    }

    public void OnClickCertificate() {
        currentMode = Mode.Certificate;
        honorCount.text = "ui_exist_certificate".Localize() + certificateInfos.Count;
        scrollTrophy.gameObject.SetActive(false);
        scrollCerificate.gameObject.SetActive(true);

        Refresh();
    }

    public void OnClickRank() {
        PopupManager.HonorRank();
    }
}
