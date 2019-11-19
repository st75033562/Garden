using UnityEngine;
using UnityEngine.UI;

public class UITeacherInfo : MonoBehaviour {
    [SerializeField]
    private Text Name;
    [SerializeField]
    private Image icon;

    public void initData (MemberInfo info) {
        Name.text = info.nickName;
        icon.sprite = UserIconResource.GetUserIcon((int)info.iconId);
    }

    public void OnClickClose() {
        gameObject.SetActive (false);
    }
}
