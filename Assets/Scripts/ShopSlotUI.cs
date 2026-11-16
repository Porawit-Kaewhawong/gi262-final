using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI slot representing a single ShopItem inside ShopUI.
/// </summary>
public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon = null;
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private TextMeshProUGUI priceText = null;
    [SerializeField] private Button buyButton = null;

    private ShopItem currentItem;
    private ShopUI parentShop;

    public void SetUp(ShopItem item, ShopUI shop)
    {
        currentItem = item;
        parentShop = shop;

        if (item == null) return;

        if (icon != null)
        {
            icon.sprite = item.itemIcon;
            icon.color = item.costType == CurrencyType.LEVEL
                ? new Color(0.55f, 0.85f, 1f, 1f)
                : new Color(1f, 0.88f, 0.25f, 1f);
        }

        if (nameText != null)
        {
            nameText.text = item.itemName;
        }

        if (priceText != null)
        {
            priceText.text = item.costType == CurrencyType.LEVEL
                ? $"{item.levelCost} Level"
                : $"{item.price} Coins";
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    public void OnClickBuy()
    {
        if (parentShop == null || currentItem == null) return;

        Player player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
        parentShop.BuyItem(player, currentItem);
    }
}
