using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LessonContentItem : MonoBehaviour {
    public InputField nameInput;
    public Image icon;
    public Sprite[] spriteIcons;
    public UIButtonToggle codeToggle;

    private PeriodItem periodContent;
    private PopupILPeriod everyLessonManager;
    private string oldName;
    private PeriodItem.State oldState;
    private bool initialized;

    void Start()
    {
        codeToggle.beforeChangingState += OnBeforeToggleCode;
    }

    public void SetData(PopupILPeriod everyLessonManager, PeriodItem data) {
        this.everyLessonManager = everyLessonManager;
        periodContent = data;

        oldState = data.state;
        oldName = data.periodItem.ItemName;

        nameInput.text = data.periodItem.ItemName;
        icon.sprite = spriteIcons[data.periodItem.ItemType];
        codeToggle.gameObject.SetActive(data.periodItem.eItemType == Period_Item_Type.ItemProject &&
                                        everyLessonManager.HasGameboard());
        if (codeToggle.gameObject.activeSelf)
        {
            codeToggle.isOn = everyLessonManager.IsCodeSelected(periodContent);
        }
    }

    bool OnBeforeToggleCode(bool isOn)
    {
        return everyLessonManager.OnBeforeToggleCodeBinding(periodContent, isOn);
    }

    public PeriodItem GetContent()
    {
        return periodContent;
    }

    public void OnClickDel() {
     //   everyLessonManager.DeleteItem(this);
    }

    public void NameValueEditor() {
        string itemName = nameInput.text.TrimEnd();
        periodContent.periodItem.ItemName = itemName;
        if (itemName != oldName && oldState == PeriodItem.State.EXISTING)
        {
            periodContent.state = PeriodItem.State.CHANGED;
        }
        else if (itemName == oldName)
        {
            periodContent.state = oldState;
        }
    }
}
