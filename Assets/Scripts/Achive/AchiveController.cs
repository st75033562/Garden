using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataAccess;
using UnityEngine.UI;

public class AchieveDataPack {
    public List<AchieveData> datas = new List<AchieveData>(4);
}
public class AchiveController : MonoBehaviour {
    [SerializeField]
    private ScrollableAreaController scrollTrophy;
    [SerializeField]
    private ScrollableAreaController scrollMedal;
    [SerializeField]
    private Button preBtn;
    [SerializeField]
    private GameObject rankGo;

    private List<AchieveData> listAchiveDatas;
    private List<MedalData> listMeadlDatas;
    private List<AchieveDataPack> achieveDatapacks = new List<AchieveDataPack>();
    private const int packCount = 4;
    // Use this for initialization
    void Start () {
        //var source = ResourceDataSource.instance;
        //AchieveData.Load(source);

        listAchiveDatas = AchieveData.getDatas();

        AchieveDataPack achieveDatapack = null;
        for(int i=0; i < listAchiveDatas.Count; i++) {
            if(i % packCount == 0) {
                achieveDatapack = new AchieveDataPack();
                achieveDatapacks.Add(achieveDatapack);
            }
            achieveDatapack.datas.Add(listAchiveDatas[i]);
        }

        scrollTrophy.InitializeWithData(achieveDatapacks);

        listMeadlDatas = MedalData.getDatas();
        scrollMedal.InitializeWithData(listMeadlDatas);
    }
	
    public void TrophyBtn(Button btn) {
        preBtn.interactable = true;
        btn.interactable = false;
        preBtn = btn;
        scrollTrophy.gameObject.SetActive(true);
        scrollMedal.gameObject.SetActive(false);
    }

    public void MedalBtn(Button btn) {
        preBtn.interactable = true;
        btn.interactable = false;
        preBtn = btn;
        scrollTrophy.gameObject.SetActive(false);
        scrollMedal.gameObject.SetActive(true);
    }

    public void OnClickRank() {
        rankGo.SetActive(true);
    }

    public void OnClickCloseRank() {
        rankGo.SetActive(false);
    }

    public void OnClickBack() {
        SceneDirector.Pop();
    }
}
