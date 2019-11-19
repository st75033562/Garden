using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentTargetUI : MonoBehaviour {
    public RectTransform targetTf;

    void OnEnable() {
        var thisRt = GetComponent<RectTransform>();
        thisRt.anchorMax = targetTf.anchorMax;
        thisRt.anchorMin = targetTf.anchorMin;
        thisRt.pivot = targetTf.pivot;

        transform.position = targetTf.position;
        transform.rotation = targetTf.rotation;
        transform.localScale = targetTf.localScale;
    }
}
