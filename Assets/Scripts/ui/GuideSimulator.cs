using RobotSimulation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuideSimulator : MonoBehaviour {
    [SerializeField]
    private UIWorkspace workspace;

    private GuideHand guideHand;

    [SerializeField]
    private Button[] setBtns;
    [SerializeField]
    private UISceneSelection sceneSelect;
    [SerializeField]
    private Simulator simulator;

    private int currentStepBtn;
	// Use this for initialization
	void Start () {

    }

    public void Show() {
        foreach(Button btn in setBtns) {
            btn.interactable = false;
        }

        simulator.LoadScene(UserManager.Instance.guideLevelData.simulator);
        currentStepBtn = 0;

        NextStep();
    }
	
    public void OnClickLoad() {
        
    }

    public void OnClickRun() {
        if(!this.enabled)
            return;
        if(currentStepBtn == 1) {
            workspace.m_OnStopRunning.AddListener(CodeRunFinish);
        }
    }

    void CodeRunFinish() {
        NextStep();
        workspace.m_OnStopRunning.RemoveListener(CodeRunFinish);
    }

    void NextStep() {
        if (!guideHand) {
            guideHand = PopupManager.Find<GuideHand>();
        }
        guideHand.BringToTop();

        if(currentStepBtn == 0) {
            guideHand.RotateByZ(-90);
            guideHand.Twinkle(setBtns[currentStepBtn].gameObject);
        } else if(currentStepBtn == 1) {
            guideHand.RotateByZ(90);
            guideHand.Twinkle(setBtns[currentStepBtn].gameObject, GuideHand.TwinkleAnchor.Right);
        }
        setBtns[currentStepBtn].interactable = true;
        
        currentStepBtn++;
    }

}
