using UnityEngine;
using System.Collections;

public class GuideMenuSelect : MonoBehaviour {
    [SerializeField]
    private GuideHand guideHand;
    [SerializeField]
    private UIMenu uiMenu;
	
    public void ShowClickHint(int menuItem) {
    }

    public void Hide()
    {
        guideHand.gameObject.SetActive(false);
    }
}
