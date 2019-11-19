using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaceLamp : UIBehaviour
{
    private RectTransform textRect;
    private RectTransform parentRect;

    public float speed = 50;
    public bool horizontal = true;

    private float textWidth;
    private float moveLength;
    private float move;
    private Vector3 textInitPostion;
    private Vector3 textAdjustPostion;

    private bool sizeChanged = true;
    private float parentWidth;

    protected override void Awake()
    {
        textRect = (RectTransform)transform;
        parentRect = (RectTransform)transform.parent;
        textInitPostion = textRect.localPosition;
    }

    public void ResetPostion()
    {
        textRect.localPosition = textInitPostion;
        move = 0;
    }

    private void InitializeAnimation()
    {
        textWidth = LayoutUtility.GetPreferredWidth(textRect);

        parentWidth = parentRect.rect.width;
        moveLength = textWidth - parentWidth;
        if (moveLength > 0)
        {
            if (horizontal)
            {
                // offset the text to align the left along the parent's left
                float offset = parentRect.rect.xMin - (textRect.rect.xMin + textInitPostion.x);
                textAdjustPostion = textInitPostion;
                textAdjustPostion.x += offset;
                UpdatePosition();
            }
            else
            {
                Debug.LogError("vertical scroll not implemented");
            }
        }
        else
        {
            ResetPostion();
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) { return; }
        sizeChanged = true;
    }

    private void UpdatePosition()
    {
        Vector3 newPos = textAdjustPostion;
        newPos.x += move;
        textRect.localPosition = newPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (sizeChanged)
        {
            sizeChanged = false;
            InitializeAnimation();
        }

        if (moveLength > 0)
        {
            if (horizontal)
            {
                move -= Time.deltaTime * speed;
                UpdatePosition();
                if (move < -textWidth)
                {
                    move = parentWidth;
                }
            }
        }
    }
}
