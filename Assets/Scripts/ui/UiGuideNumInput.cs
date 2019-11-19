using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiGuideNumInput : MonoBehaviour {
    [SerializeField]
    private Button[] steps_1;
    [SerializeField]
    private Button[] steps_2;
    [SerializeField]
    private Button[] steps_3;
    [SerializeField]
    private Button[] steps_4;

    private GuideHand guideHand;

    [SerializeField]
    private Button[] allBut;
    [SerializeField]
    private Text textInfo;
    private int index;
    private Button preBtn;

    private List<Button[]> stepLevel_1 = new List<Button[]>();
    private Button[] steps;

    public void Init()
    {
        stepLevel_1.Add(steps_1);
        stepLevel_1.Add(steps_2);
        stepLevel_1.Add(steps_3);
        stepLevel_1.Add(steps_4);
    }

    public void EnableAllButtons(bool enabled) {
        foreach(Button btn in allBut) {
            btn.interactable = enabled;
        }
    }

    public void SetStep(int index) {
        EnableAllButtons(false);
        this.index = 0;
        steps = stepLevel_1[index];
        ShowHand();
    }

    public void ShowInfo(string str) {
        textInfo.gameObject.SetActive(true);
        textInfo.text = str;
    }

    public void OnClick(string str) {
        if(!this.enabled)
            return;
        ShowHand();
    }

    void ShowHand() {
        if(index < steps.Length) {
            if (!guideHand) {
                guideHand = PopupManager.Find<GuideHand>();
            }
            guideHand.BringToTop();

            if(preBtn != null)
                preBtn.interactable = false;
            preBtn = steps[index];
            steps[index].interactable = true;
            guideHand.Twinkle(steps[index].gameObject);
            index++;
        }
    }

    void OnDisable() {
        EnableAllButtons(true);
        textInfo.gameObject.SetActive(false);
    }
}
