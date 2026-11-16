using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Shop UI, instantiates ShopSlotUI prefabs, and processes item purchases using Coins or Levels.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Shop Configuration")]
    public List<ShopItem> shopItems = new List<ShopItem>();
    public GameObject shopSlotPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel = null;
    [SerializeField] private Transform slotsContainer = null;
    [SerializeField] private Button closeShopButton = null;

    private readonly List<GameObject> spawnedSlots = new List<GameObject>();

    private void Awake()
    {
        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveAllListeners();
            closeShopButton.onClick.AddListener(HideShop);
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void SetUpShop()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i]);
            }
        }
        spawnedSlots.Clear();

        if (shopSlotPrefab == null || slotsContainer == null) return;

        for (int i = 0; i < shopItems.Count; i++)
        {
            ShopItem item = shopItems[i];
            if (item == null) continue;

            GameObject slotObj = Instantiate(shopSlotPrefab, slotsContainer);
            slotObj.SetActive(true);

            ShopSlotUI slotUI = slotObj.GetComponent<ShopSlotUI>();
            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<ShopSlotUI>();
            }

            slotUI.SetUp(item, this);
            spawnedSlots.Add(slotObj);
        }
    }

    public void BuyItem(Player player, ShopItem item)
    {
        if (player == null || item == null) return;

        if (item.costType == CurrencyType.COIN)
        {
            if (player.coinCurrency < item.price)
            {
                return;
            }
            player.coinCurrency -= item.price;
        }
        else if (item.costType == CurrencyType.LEVEL)
        {
            if (player.level <= item.levelCost)
            {
                return;
            }
            player.DeductLevel(item.levelCost);
        }

        ApplyPurchasedItemEffect(player, item);
    }

    public void ShowShop()
    {
        Player player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.hasKilledMonster = player.AreAllEnemiesDefeated();
            if (!player.hasKilledMonster)
            {
                return;
            }
        }

        SetUpShop();
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
    }

    public void HideShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void ApplyPurchasedItemEffect(Player player, ShopItem item)
    {
        string lower = (item.itemName ?? string.Empty).ToLowerInvariant();
        if (lower.Contains("health") || lower.Contains("potion") || lower.Contains("heal"))
        {
            player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + 50f);
        }
        else if (lower.Contains("speed"))
        {
            player.ApplyStatusEffect(new StatEffect("Speed Boost", 12f, 1.35f, false));
        }
        else if (lower.Contains("poison") || lower.Contains("dot"))
        {
            player.enablePoisonShuriken = true;
            player.attackDamage += 5f;
        }
        else if (lower.Contains("stun"))
        {
            player.enableStunShuriken = true;
            player.attackDamage += 5f;
        }
        else
        {
            player.maxHealth += 20f;
            player.currentHealth = player.maxHealth;
            player.attackDamage += 8f;
        }
    }
}
