using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using RobotSimulation;

public class GuideInputBackData {
    public GameObject go;
    public string inputText;

    public GuideInputBackData(GameObject go , string inputText) {
        this.go = go;
        this.inputText = inputText;
    }
}
public class GuideController : MonoBehaviour, IClosestConnectionFilter {
    private UiGuideNumInput uiGuideNumInput;

    private GuideHand guideHand;

    [SerializeField]
    private GuideSimulator guideSimulator;
    [SerializeField]
    private RectTransform leftContent;
    [SerializeField]
    private Text noticeText;
    [SerializeField]
    private RectTransform[] runSteps;
    [SerializeField]
    private GameObject[] buttonsToHide;
    [SerializeField]
    private GameObject[] notActiveGos;
    [SerializeField]
    private Button backBtn;
    [SerializeField]
    private Button runBtn;
    [SerializeField]
    private Button simulationBtn;
    [SerializeField]
    private UIWorkspace uiManager;
    [SerializeField]
    private CodePanelManager codePanelManager;

    private GuideLevelData guideLevelData;
    private int currentStep;
    private const float leftContentSpace = 513;
    private float leftContentMaxY;
    private GameObject originGo;
    private GameObject targetGo;
    private bool showNumberGuide = true;
    private bool finish;

    private int curLevel = 1;
    private int level1Solt1Count;
    private int imageStep_1;
    private bool effectCodeStop;
    private StrawBerryData[] strawBerryDatas = {
        new StrawBerryData(StrawBerryData.State.EffectDisplay, "guide_text_step_1"),
        new StrawBerryData(StrawBerryData.State.ShortDistance, "guide_text_step_2"),
        new StrawBerryData(StrawBerryData.State.LongDistance, "guide_text_step_3")
    };

    private Action<NodePluginsBase, UIDialog> onBeginInputEdit;
    private Action<NodePluginsBase, UIDialog> onEndInputEdit;

    // Use this for initialization
    public void Initialize() {
        if(UserManager.Instance.appRunModel != AppRunModel.Guide) {
            CloseGuide();
            this.enabled = false;
            return;
        }

        uiManager.m_filter.InputEnabled = false;
        uiManager.ShowCloseNodeListButton(false);

        simulationBtn.gameObject.SetActive(false);
        curLevel = UserManager.Instance.guideLevel;
        guideLevelData = UserManager.Instance.guideLevelData;

        guideHand = (GuideHand)PopupManager.Create("popupGuideHand");

        if(curLevel != 1) {  //第一关卡显示运行效果图
            showNumberGuide = false;
        } else {
            PopupManager.StrawberryMap(strawBerryDatas[currentStep]);
        }

        foreach (var btn in buttonsToHide)
        {
            btn.SetActive(false);
        }

        uiManager.m_Toolbar.MessageButtonVisible = false;

        foreach(GameObject go in notActiveGos) {
            go.SetActive(false);
        }
        backBtn.interactable = false;
        runBtn.interactable = false;

        EventBus.Default.AddListener(EventId.GuideInput, InputCallBack);
        EventBus.Default.AddListener(EventId.GuideInvalidInput, InvalidInputCallBack);
        uiManager.OnEndDrag += OnEndDraggingNode;
        uiManager.ConnectionFilter = this;
        uiManager.OnDidLoadCode += OnDidLoadCode;
        uiManager.m_OnStopRunning.AddListener(CodeRunFinish);
        if(UserManager.Instance.isSimulationModel) {
            guideSimulator.enabled = true;
        }
    }

    void CloseGuide() {
        showNumberGuide = false;
        showOrCloseNotice(false);
    }

    void NextNode() {
        if(currentStep >= guideLevelData.data.Count) {
            guideHand.Hide();
            noticeText.transform.parent.gameObject.SetActive(false);

            if (uiGuideNumInput)
            {
                uiGuideNumInput.enabled = false;
            }
            showNumberGuide = false;
            finish = true;

            guideHand.Show();
            RunStep();
            return;
        }
        SetBlockLevel();
        FunctionNode node = GetTemplateNodeAndDisableOthers(guideLevelData.data[currentStep].nodeId);
        originGo = node.gameObject;

        float scale = leftContent.localScale.y;
        leftContentMaxY = leftContent.rect.height * scale - leftContentSpace - 90;
        Vector2 v2 = node.GetComponent<RectTransform>().anchoredPosition;
       
        v2.y = -v2.y - leftContentSpace;
        v2 *= scale;
        if(v2.y < 0)
            v2.y = 0;
        else if(v2.y > leftContentMaxY)
            v2.y = leftContentMaxY;
        leftContent.anchoredPosition = v2;

     //   transform.position = go.transform.position;

        guideHand.Move(originGo , targetGo);
        noticeText.text = guideLevelData.data[currentStep].notice.Localize();
    }

    FunctionNode GetTemplateNodeAndDisableOthers(int nodeId)
    {
        var nodes = uiManager.m_NodeTempList.CurrentNodes;
        FunctionNode targetNode = null;
        foreach (var node in nodes)
        {
            if (node.NodeTemplateId == nodeId)
            {
                targetNode = node;
                node.Interactable = true;
            }
            else
            {
                node.Interactable = false;
            }
        }
        return targetNode;
    }

    void OnEndDraggingNode(FunctionNode node, NodeDropResult dropResult) {
        //是点击事件命令行时
        if (node is MainNode && targetGo)
        {
            return;
        }

        FunctionNode functionNode = node.GetComponent<FunctionNode>();
        if(functionNode.PrevNode == null) {
            functionNode.ChainedDelete();
            return;
        }

        int index = 0;
        foreach(MainNode mainNode in uiManager.CodePanel.GetMainNodes()) {
            FunctionNode nextNode = mainNode.NextNode;
            while(nextNode) {
                if(index >= guideLevelData.data.Count || nextNode.NodeTemplateId != guideLevelData.data[index++].nodeId) {
                    functionNode.ChainedDelete();
                    return;
                }
                nextNode = nextNode.NextNode;
            }
        }
        if(originGo != null) {
            FunctionNode origionNode = originGo.GetComponent<FunctionNode>();
            origionNode.Interactable = false;
        }

        functionNode.Draggable = false;
        targetGo = node.gameObject;
        currentStep = index - 1;
        if(currentStep < 0)
            currentStep = 0;
        OperationSlot();
    }

    void OnDidLoadCode(UIWorkspace workspace)
    {
        foreach (var node in uiManager.CodePanel.Nodes)
        {
            if (node is MainNode)
            {
                targetGo = node.gameObject;
                break;
            }
        }

        if (targetGo)
        {
            NextNode();
        }
        else
        {
            Debug.LogFormat("target node not found");
        }
    }

    private GameObject pluginGo;
    void OperationSlot() {
        bool containSlot = false;
        GameObject go = null;

        if(curLevel == 1 && level1Solt1Count == 0) {
            RunStep();
            level1Solt1Count++;
            return;
        } else if(curLevel == 1 && level1Solt1Count < 2) {
            var node = targetGo.GetComponent<FunctionNode>();
            go = node.GetSlotPlugin(1).gameObject;

            onBeginInputEdit = (plugin, dialogType) => {
                if(go != plugin.gameObject) {
                    showOrCloseNotice(true);
                    return;
                }
                NodePluginsBase.onBeginInputEdit -= onBeginInputEdit;

                InitGuideNumInput();
                uiGuideNumInput.enabled = true;
                uiGuideNumInput.SetStep(level1Solt1Count);
                showOrCloseNotice(false);

                onEndInputEdit = delegate {
                    showOrCloseNotice(true);
                    NodePluginsBase.onEndInputEdit -= onEndInputEdit;
                };
                NodePluginsBase.onEndInputEdit += onEndInputEdit;
            };

            NodePluginsBase.onBeginInputEdit += onBeginInputEdit;

            containSlot = true;
        } else if(guideLevelData.data[currentStep].pluginDatas.Count > 0) {
            var pluginData = guideLevelData.data[currentStep].pluginDatas[0];
            go = targetGo.GetComponentsInChildren(Type.GetType(pluginData.pluginScript))[pluginData.index].gameObject;

            onBeginInputEdit = (plugin, dialogType) => {
                if(go != plugin.gameObject) {
                    showOrCloseNotice(true);
                    return;
                }
                NodePluginsBase.onBeginInputEdit -= onBeginInputEdit;
                
                if(dialogType == UIDialog.UIMenuDialog) {
                    ShowClickHintForMenuItem(pluginData.handIndex);
                } else if(dialogType == UIDialog.UINumInputDialog) {
                    if(curLevel == 1 && level1Solt1Count <= 3) {
                        InitGuideNumInput();
                        uiGuideNumInput.enabled = true;
                        uiGuideNumInput.SetStep(level1Solt1Count);
                        level1Solt1Count++;
                    } else {
                        showOrCloseNotice(false);
                    }
                }

                onEndInputEdit = delegate {
                    showNumberGuide = false;
                    showOrCloseNotice(true);
                    NodePluginsBase.onEndInputEdit -= onEndInputEdit;
                };
                NodePluginsBase.onEndInputEdit += onEndInputEdit;
            };

            NodePluginsBase.onBeginInputEdit += onBeginInputEdit;

            guideLevelData.data[currentStep].pluginDatas.RemoveAt(0);
            containSlot = true;
        }

        pluginGo = go;
        if(containSlot) {
            guideHand.Twinkle(go);
            return;
        }

        currentStep++;
        NextNode();
    }

    private void ShowClickHintForMenuItem(int itemIndex)
    {
        var uiMenu = PopupManager.Find<UIMenuDialog>().m_Menu;
        uiMenu.SetPostionToVisibleCell(itemIndex);
        guideHand.BringToTop();
        guideHand.Twinkle(uiMenu.GetItem(itemIndex).gameObject);
    }

    private void InitGuideNumInput()
    {
        if (!uiGuideNumInput)
        {
            uiGuideNumInput = UnityEngine.Object.FindObjectOfType<UiGuideNumInput>();
            uiGuideNumInput.Init();
        }
    }

    void InputCallBack(object obj) {
        if(pluginGo != ((GuideInputBackData)obj).go)
            return;
        InvalidInputCallBack(null);
    }

    void InvalidInputCallBack(object obj) {
        if(curLevel == 1 && level1Solt1Count < 2) {
            RunStep();
            showOrCloseNotice(true);
            level1Solt1Count++;
            return;
        }
        OperationSlot();
    }

    void SetBlockLevel() {
        uiManager.BlockLevel = (BlockLevel)UserManager.Instance.guideLevelData.data[currentStep].blockLevel;
        uiManager.m_NodeTempList.ShowNodeByFilter((NodeCategory)UserManager.Instance.guideLevelData.data[currentStep].nodeType);
    }

    void OnDestroy() {
        EventBus.Default.RemoveListener(EventId.GuideInput, InputCallBack);
        EventBus.Default.RemoveListener(EventId.GuideInvalidInput, InvalidInputCallBack);
        if (uiManager)
        {
            uiManager.OnEndDrag -= OnEndDraggingNode;
            uiManager.OnDidLoadCode -= OnDidLoadCode;
            uiManager.ConnectionFilter = null;
            uiManager.m_OnStopRunning.RemoveListener(CodeRunFinish);
        }
        if (uiManager.m_NodeTempList)
        {
            foreach (var node in uiManager.m_NodeTempList.CurrentNodes)
            {
                node.Interactable = true;
            }
        }
        NodePluginsBase.onBeginInputEdit -= onBeginInputEdit;
        NodePluginsBase.onEndInputEdit -= onEndInputEdit;

        PopupManager.Close(guideHand);
    }

    void showOrCloseNotice(bool show) {
        if(!showNumberGuide && guideHand) {
            guideHand.gameObject.SetActive(show);
        }
        noticeText.transform.parent.gameObject.SetActive(show);
    }

    public void RunStep() {
        if(UserManager.Instance.isSimulationModel) {
            codePanelManager.ShowSimulator();
            guideSimulator.Show();
        } else {
            runBtn.interactable = true;
            guideHand.RotateByZ(90);
            guideHand.Twinkle(runSteps[0].gameObject);
            effectCodeStop = true;
        }
    }

    public void CodeRunFinish() {
        if(!effectCodeStop) 
            return;
        effectCodeStop = false;
        CodeRunFinishNext();
    }

    void CodeRunFinishNext() {
        if(finish) {
            guideHand.gameObject.SetActive(false);
            PopupManager.GoodJob(() => {
                backBtn.interactable = true;
                guideHand.gameObject.SetActive(true);
                guideHand.Twinkle(runSteps[1].gameObject, GuideHand.TwinkleAnchor.Right);
            });
        } else {
            if(curLevel == 1 && level1Solt1Count <= 2) {
                PopupManager.StrawberryMap(strawBerryDatas[++imageStep_1], () => {
                    guideHand.RotateByZ(0);
                    OperationSlot();
                });
            } else {
                PopupManager.StrawberryMap(strawBerryDatas[currentStep], () => {
                    guideHand.RotateByZ(0);
                    NextNode();
                });
            }
        }
    }
    public void SimulationClose() {
        if(!this.enabled)
            return;
        CodeRunFinishNext();
    }

    public void OnClickRun() {
        if(!this.enabled)
            return;
        runBtn.interactable = false;
    }

    bool IClosestConnectionFilter.Filter(Connection source, Connection target)
    {
        return target.node.gameObject != targetGo || target.type != ConnectionTypes.Bottom;
    }
}
