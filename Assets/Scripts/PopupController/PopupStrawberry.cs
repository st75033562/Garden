using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StrawBerryData{

    public enum State {
        EffectDisplay,
        ShortDistance,
        LongDistance,
    }

    public State state;
    public string explain;

    public StrawBerryData(State state , string explain) {
        this.state = state;
        this.explain = explain;
    }
}
public class PopupStrawberry : PopupController { 
    [SerializeField]
    private GameObject arrowGo;
    [SerializeField]
    private Text textExplain;
    [SerializeField]
    private GameObject robotGo;

    public float guidePathInterval = 0.4f;
    private Vector3[] movePostions = { new Vector3(22, -500,0), new Vector3(22, 190, 0) };
    private GameObject twinkleGo;
    // Use this for initialization
    protected override void Start () {
        StrawBerryData strawBerry = (StrawBerryData)payload;
        textExplain.text = strawBerry.explain.Localize();

        switch(strawBerry.state) {
            case StrawBerryData.State.EffectDisplay:
                SetTwinkleGo(arrowGo);
                break;
            case StrawBerryData.State.ShortDistance:
                SetTwinkleGo(robotGo);
                robotGo.transform.localPosition = movePostions[0];
                break;
            case StrawBerryData.State.LongDistance:
                SetTwinkleGo(robotGo);
                robotGo.transform.localPosition = movePostions[1];
                break;
        }
        StartCoroutine(EffectDisplay());
    }

    void SetTwinkleGo(GameObject go) {
        if(go == arrowGo) {
            robotGo.SetActive(false);
            arrowGo.SetActive(true);
        } else {
            robotGo.SetActive(true);
            arrowGo.SetActive(false);
        }
        twinkleGo = go;
    }

    // Update is called once per frame
    void Update () {
	
	}

    IEnumerator EffectDisplay() {
        while(true) {
            twinkleGo.SetActive(!twinkleGo.activeSelf);
            yield return new WaitForSeconds(guidePathInterval);
        }
    }
}
