using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ShopItem currentItem;
    private ShopUI parentShop;

    public void SetUp(ShopItem item, ShopUI shop)
    {
        currentItem = item;
        parentShop = shop;
        if (item == null)
        {
            return;
        }

        if (icon != null)
        {
            icon.sprite = item.itemIcon;
            if (item.costType == CurrencyType.LEVEL)
            {
                icon.color = new Color(0.55f, 0.85f, 1f, 1f);
            }
            else
            {
                icon.color = new Color(1f, 0.88f, 0.25f, 1f);
            }
        }

        if (nameText != null)
        {
            nameText.text = item.itemName;
        }

        if (priceText != null)
        {
            if (item.costType == CurrencyType.LEVEL)
            {
                priceText.text = $"{item.levelCost} Level";
            }
            else
            {
                priceText.text = $"{item.price} Coins";
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    public void OnClickBuy()
    {
        if (parentShop == null || currentItem == null)
        {
            return;
        }
        parentShop.BuyItem(Player.Instance, currentItem);
    }
}
