using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuideHand : PopupController {
    [SerializeField]
    private Image imageHand;

    private RectTransform startRt;
    private RectTransform targetRt;
    private AniSate aniState;
    private float step;

    public float duration = 0.5f;
    public float moveSpeed = 390;
    public float nearTargetDis = 0.2f;

    private Quaternion rotation;
    private TwinkleAnchor twinkleAnchor;
    public enum TwinkleAnchor {
        Core,
        Right
    }
    enum AniSate {
        Empty,
        Move,
        Twinkle
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();
        rotation = imageHand.transform.rotation;
    }

    protected override void OnEnable() {
        base.OnEnable();
        if(aniState == AniSate.Twinkle) {
            StartCoroutine(TwinkleAni());
        }
    }

    // Update is called once per frame
    void Update() {
        if(aniState == AniSate.Move) {
            step = moveSpeed * Time.deltaTime;
            imageHand.transform.position = Vector3.MoveTowards(imageHand.transform.position, targetRt.RectCoreWorldPos(), step);
            if(Vector3.Distance(imageHand.transform.position , targetRt.RectCoreWorldPos()) < nearTargetDis) {
                imageHand.transform.position = startRt.RectCoreWorldPos();
            }
        }
    }

    public void Move(GameObject startGo, GameObject targetGo) {
        startRt = startGo.GetComponent<RectTransform>();
        targetRt = targetGo.GetComponent<RectTransform>();
        imageHand.transform.position = startRt.RectCoreWorldPos();
        imageHand.enabled = true;
        aniState = AniSate.Move;
    }

    public void Twinkle(GameObject targetGo, TwinkleAnchor anchor = TwinkleAnchor.Core) {
        twinkleAnchor = anchor;
        targetRt = targetGo.GetComponent<RectTransform>();
        switch(anchor) {
            case TwinkleAnchor.Core:
                imageHand.transform.position = targetRt.RectCoreWorldPos();
                break;
            case TwinkleAnchor.Right:
                imageHand.transform.position = targetRt.RectRightWorldPos();
                break;
        }
        
        if(aniState == AniSate.Twinkle)
            return;
        aniState = AniSate.Twinkle;
        StartCoroutine(TwinkleAni());
    }

    public void RotateByZ(float angle) {
        imageHand.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void ReSet() {
        imageHand.transform.rotation = rotation;
    }
    IEnumerator TwinkleAni() {
        while (aniState == AniSate.Twinkle) {
            if (targetRt) {
                switch(twinkleAnchor) {
                    case TwinkleAnchor.Core:
                        imageHand.transform.position = targetRt.RectCoreWorldPos();
                        break;
                    case TwinkleAnchor.Right:
                        imageHand.transform.position = targetRt.RectRightWorldPos();
                        break;
                }
                imageHand.enabled = !imageHand.enabled;
            }
            yield return new WaitForSeconds(duration);
        }
    }

    Vector3 CenterWorldPostion(RectTransform rectT) {
        Vector3[] corners = new Vector3[4];
        rectT.GetWorldCorners(corners);
        Vector3 result = new Vector3();
        result.x = (corners[0].x + corners[3].x) / 2;
        result.y = (corners[0].y + corners[1].y) / 2;
        result.z = corners[0].z;
        return result;
    }

    public void Hide() {
        imageHand.enabled = false;
    }

    public void Show() {
        imageHand.enabled = true;
    }

    public override bool OnKey(KeyEventArgs eventArgs)
    {
        return false;
    }
}
