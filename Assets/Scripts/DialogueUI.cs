using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic Dialogue System UI that adapts NPC dialogue based on whether the Player has defeated all enemies,
/// and unlocks the ShopUI only after all enemies are defeated.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Dynamic Dialogue Lists")]
    public List<string> defaultDialogues = new List<string>
    {
        "Hello there."
    };

    public List<string> noKillDialogues = new List<string>
    {
        "There are still enemies nearby.",
        "Defeat all the enemies first, and I will open the shop."
    };

    public List<string> hasKilledDialogues = new List<string>
    {
        "You defeated all the enemies!",
        "My shop is open now if you want to buy anything."
    };

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel = null;
    [SerializeField] private TextMeshProUGUI dialogueText = null;
    [SerializeField] private ShopUI linkedShopUI = null;
    [SerializeField] private Button openShopButton = null;
    [SerializeField] private Button closeDialogueButton = null;

    private int currentDialogueIndex = 0;

    private void Awake()
    {
        if (openShopButton != null)
        {
            openShopButton.onClick.RemoveAllListeners();
            openShopButton.onClick.AddListener(() => OpenShop(linkedShopUI));
        }

        if (closeDialogueButton != null)
        {
            closeDialogueButton.onClick.RemoveAllListeners();
            closeDialogueButton.onClick.AddListener(CloseDialogue);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void StartDialogue(Player player)
    {
        currentDialogueIndex = 0;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        DisplayDynamicText(player);
    }

    public void DisplayDynamicText(Player player)
    {
        bool allEnemiesDefeated = false;
        List<string> activeList = defaultDialogues;

        if (player != null)
        {
            player.hasKilledMonster = player.AreAllEnemiesDefeated();
            allEnemiesDefeated = player.hasKilledMonster;

            if (allEnemiesDefeated && hasKilledDialogues != null && hasKilledDialogues.Count > 0)
            {
                activeList = hasKilledDialogues;
            }
            else if (!allEnemiesDefeated && noKillDialogues != null && noKillDialogues.Count > 0)
            {
                activeList = noKillDialogues;
            }
        }

        if (openShopButton != null)
        {
            openShopButton.gameObject.SetActive(allEnemiesDefeated);
        }

        if (activeList == null || activeList.Count == 0)
        {
            if (dialogueText != null) dialogueText.text = "...";
            return;
        }

        int idx = Mathf.Clamp(currentDialogueIndex, 0, activeList.Count - 1);
        if (dialogueText != null)
        {
            dialogueText.text = activeList[idx];
        }
    }

    public void DisplayText(Player player)
    {
        DisplayDynamicText(player);
    }

    public void AdvanceOrClose(Player player, NPC sourceNpc = null)
    {
        List<string> activeList = defaultDialogues;
        if (player != null)
        {
            player.hasKilledMonster = player.AreAllEnemiesDefeated();
            if (player.hasKilledMonster && hasKilledDialogues != null && hasKilledDialogues.Count > 0)
                activeList = hasKilledDialogues;
            else if (!player.hasKilledMonster && noKillDialogues != null && noKillDialogues.Count > 0)
                activeList = noKillDialogues;
        }

        currentDialogueIndex++;
        if (activeList != null && currentDialogueIndex < activeList.Count)
        {
            DisplayDynamicText(player);
        }
        else
        {
            CloseDialogue();
            if (sourceNpc != null)
            {
                sourceNpc.isTalking = false;
            }
        }
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        NPC[] npcs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] != null) npcs[i].isTalking = false;
        }
    }

    public void OpenShop(ShopUI shopUI)
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

        if (shopUI == null)
        {
            shopUI = linkedShopUI != null ? linkedShopUI : FindFirstObjectByType<ShopUI>(FindObjectsInactive.Include);
        }

        CloseDialogue();
        if (shopUI != null)
        {
            shopUI.ShowShop();
        }
    }
}
