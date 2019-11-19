using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class GuideLevelData {
    public int leve;
    public List<GuideLeveRowData> data = new List<GuideLeveRowData>();
    public string simulator;
}

public class GuideLeveRowData {
    public int nodeId;
    public int blockLevel;
    public int nodeType;
    public string notice;
    public List<GuideLevelPluginData> pluginDatas = new List<GuideLevelPluginData>();
}

public class GuideLevelPluginData {
    public string pluginScript;
    public int index;
    public int handIndex;
}
public class GuideLevelInfo {
    public string lessonName;
    public string lessonGoal;
    public string simulator;
}

public class SmartClassController : MonoBehaviour {
    [SerializeField]
    private GameObject classInfoGo;
    [SerializeField]
    private Text lessonTitle;
    [SerializeField]
    private Text lessonName;
    [SerializeField]
    private Text lessonGoal;
    [SerializeField]
    private Text[] cellTexts;

    private List<GuideLevelData> GuideLevelDatas;
    private Dictionary<int, GuideLevelInfo> guideLevelInfos;
    private string[] nodeNames;
    // Use this for initialization
    void Start() {
        if(GuideLevelDatas == null)
            ParseNodeData();
        if(guideLevelInfos == null)
            ParseLevelInfoData();

        for(int i = 0; i < cellTexts.Length; i++) {
            cellTexts[i].text = guideLevelInfos[i + 1].lessonName.Localize();
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public void ClickCell(int index) {
        UserManager.Instance.guideLevel = index + 1;
        UserManager.Instance.guideLevelData = GuideLevelDatas[index];
        classInfoGo.SetActive(true);
        lessonTitle.text = "text_lesson_key".Localize() + " " + (index + 1);
        lessonName.text = guideLevelInfos[index + 1].lessonName.Localize();
        lessonGoal.text = guideLevelInfos[index + 1].lessonGoal.Localize();
        UserManager.Instance.guideLevelData.simulator = guideLevelInfos[index + 1].simulator;
    }

    public void ClikcGo() {
        SceneDirector.Push("Main");
    }

    void ParseNodeData() {
        GuideLevelDatas = new List<GuideLevelData>();
        TextAsset levelString = Resources.Load<TextAsset>("Data/level");
        string[] mData = levelString.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        nodeNames = mData[0].Split('\t');

        for(int i = 1; i < mData.Length; ++i) {
            string[] mNodeInfo = mData[i].Split('\t');
            GuideLeveRowData data = new GuideLeveRowData();
            GuideLevelData levelData = getGuideLevelDataByLevel(int.Parse(mNodeInfo[0]));
            data.nodeId = int.Parse(mNodeInfo[GetIndex("nodeId")]);
            data.blockLevel = int.Parse(mNodeInfo[GetIndex("blockLevel")]);
            data.nodeType = int.Parse(mNodeInfo[GetIndex("nodeType")]);

            if(mNodeInfo[GetIndex("pluginScript_1")] != "null") {
                GuideLevelPluginData pluginData = new GuideLevelPluginData();
                pluginData.pluginScript = mNodeInfo[GetIndex("pluginScript_1")];
                pluginData.index = int.Parse(mNodeInfo[GetIndex("pluginIndex_1")]);
                pluginData.handIndex = int.Parse(mNodeInfo[GetIndex("pluginHand_1")]);
                data.pluginDatas.Add(pluginData);
            }
            if(mNodeInfo[GetIndex("pluginScript_2")] != "null") {
                GuideLevelPluginData pluginData = new GuideLevelPluginData();
                pluginData.pluginScript = mNodeInfo[GetIndex("pluginScript_2")];
                pluginData.index = int.Parse(mNodeInfo[GetIndex("pluginIndex_2")]);
                pluginData.handIndex = int.Parse(mNodeInfo[GetIndex("pluginHand_2")]);
                data.pluginDatas.Add(pluginData);
            }
            if(mNodeInfo[GetIndex("pluginScript_3")] != "null") {
                GuideLevelPluginData pluginData = new GuideLevelPluginData();
                pluginData.pluginScript = mNodeInfo[GetIndex("pluginScript_3")];
                pluginData.index = int.Parse(mNodeInfo[GetIndex("pluginIndex_3")]);
                pluginData.handIndex = int.Parse(mNodeInfo[GetIndex("pluginHand_3")]);
                data.pluginDatas.Add(pluginData);
            }

            data.notice = mNodeInfo[GetIndex("notice")];

            levelData.data.Add(data);
        }
    }

    int GetIndex(string str) {
        for(int i = 0; i < nodeNames.Length; i++) {
            if(nodeNames[i] == str)
                return i;
        }
        return -1;
    }

    void ParseLevelInfoData() {
        guideLevelInfos = new Dictionary<int, GuideLevelInfo>();
        TextAsset levelString = Resources.Load<TextAsset>("Data/level_data");
        string[] mData = levelString.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        for(int i = 1; i < mData.Length; ++i) {
            string[] mNodeInfo = mData[i].Split('\t');
            GuideLevelInfo data = new GuideLevelInfo();
            data.lessonName = mNodeInfo[1];
            data.lessonGoal = mNodeInfo[2];
            data.simulator = mNodeInfo[3];

            guideLevelInfos.Add(int.Parse(mNodeInfo[0]), data);
        }
    }

    GuideLevelData getGuideLevelDataByLevel(int level) {
        GuideLevelData guideLevelData = null;
        if(GuideLevelDatas.Count > 0) {
            guideLevelData = GuideLevelDatas.Find((x) => { return x.leve == level; });
        }
        if(guideLevelData == null) {
            guideLevelData = new GuideLevelData();
            guideLevelData.leve = level;

            GuideLevelDatas.Add(guideLevelData);
        }
        return guideLevelData;
    }

    public void CloseInfo() {
        classInfoGo.SetActive(false);
    }

    public void ClickBack() {
        SceneDirector.Pop();
    }

    public void ClickHome() {
        Utils.GotoHomeScene();
    }
}