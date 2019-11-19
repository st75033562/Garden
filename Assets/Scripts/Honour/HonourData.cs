using DataAccess;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CertificateData{
    public int id;
    public string assetBundleName;
    public string assetName;
    public string resultBundle;
    public string resultName;
    public string notifyBundle;
    public string notifyName;

    private static Dictionary<int, CertificateData> certificateDatas;

    public static void Load(IDataSource source) {
        certificateDatas = JsonMapperUtils.ToDictFromList<int, CertificateData>(source.Get("certificate"), x => x.id);
    }

    public static CertificateData GetCerticateData(int keyId) {
        CertificateData data;
        certificateDatas.TryGetValue(keyId, out data);
        return data;
    }

    public static List<CertificateData> GetAllCerticateData() {
        return certificateDatas.Values.ToList();
    }

}

public class TrophyData {
    public enum Type {
        Body ,
        Bottom,
        Handle,
        Decorate
    }

    public int id;
    public string assetBundleName;
    public string assetNameGold;
    public string assetNameSilver;
    public string assetNameBronze;
    public int type;
    public int handleY;

    private static Dictionary<int, TrophyData> trophyDatas;

    public static void Load(IDataSource source) {
        trophyDatas = JsonMapperUtils.ToDictFromList<int, TrophyData>(source.Get("trophy"), x => x.id);
    }

    public static TrophyData GetTrophyData(int keyId) {
        TrophyData data;
        trophyDatas.TryGetValue(keyId, out data);
        return data;
    }

    public static List<TrophyData> GetAllTrophyData() {
        return trophyDatas.Values.ToList();
    }
}

public class TrophyResultData {
    public int id;
    public string assetBundleName;
    public string assetName;
    public string previewBundleName;
    public string previewAssetName;
    public string rankAssetName;

    private static Dictionary<int, TrophyResultData> resultData;

    public static void Load(IDataSource source) {
        resultData = JsonMapperUtils.ToDictFromList<int, TrophyResultData>(source.Get("trophy_result"), x => x.id);
    }

    public static TrophyResultData GeTrophyData(int keyId) {
        TrophyResultData data;
        resultData.TryGetValue(keyId, out data);
        return data;
    }

    public static List<TrophyResultData> GetAllTrophyData() {
        return resultData.Values.ToList();
    }

}