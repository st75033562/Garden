using UnityEngine;
using UnityEngine.UI;

public class PopupPurchase : PopupController
{
    public Text m_buyHintText;
    public Text m_notEnoughGoldText;

    public void Initialize(string buyHint, int price, string buyButtonText)
    {
        var enoughCoin = UserManager.Instance.Coin >= price;
        m_buyHintText.text = buyHint;
        m_notEnoughGoldText.gameObject.SetActive(!enoughCoin);
        rightText = enoughCoin ? buyButtonText : "ui_purchase_recharge_button_text".Localize();
    }

    public override void OnRightClose()
    {
        // if coins are enough
        if (!m_notEnoughGoldText.gameObject.activeSelf)
        {
            base.OnRightClose();
        }
        else
        {
            // TODO: show recharge dialog
            base.OnLeftClose();
        }
    }
}
