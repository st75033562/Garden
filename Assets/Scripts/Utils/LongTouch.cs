using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class LongTouch : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler ,IPointerDownHandler , IPointerUpHandler{

    [SerializeField]
    private UnityEvent onClick;

    public float continueTime = 1.8f;

    private float time;

    private bool onPoint;

    // Use this for initialization
    void Start () {
	
	}

    public void reset () {
        time = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if(onPoint) {
            if(Time.time - time > continueTime) {
                onClick.Invoke ();
                onPoint = false;
            }
        }
	}

    public void OnPointerEnter (PointerEventData eventData)
    {
        
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onPoint = true;
        reset();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPoint = false;
    }
}
