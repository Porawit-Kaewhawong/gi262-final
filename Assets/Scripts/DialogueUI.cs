using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public ShopUI linkedShopUI;
    public Button openShopButton;
    public Button closeDialogueButton;

    private int currentDialogueIndex = 0;

    private void Awake()
    {
        if (openShopButton != null)
        {
            openShopButton.onClick.AddListener(() => OpenShop(linkedShopUI));
        }

        if (closeDialogueButton != null)
        {
            closeDialogueButton.onClick.AddListener(CloseDialogue);
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
        List<string> activeList = GetActiveList(player, out bool allEnemiesDefeated);

        if (openShopButton != null)
        {
            openShopButton.gameObject.SetActive(allEnemiesDefeated);
        }

        if (dialogueText == null)
        {
            return;
        }

        if (activeList == null || activeList.Count == 0)
        {
            dialogueText.text = "...";
            return;
        }

        int idx = Mathf.Clamp(currentDialogueIndex, 0, activeList.Count - 1);
        dialogueText.text = activeList[idx];
    }

    public void DisplayText(Player player)
    {
        DisplayDynamicText(player);
    }

    public void AdvanceOrClose(Player player, NPC sourceNpc = null)
    {
        List<string> activeList = GetActiveList(player, out _);

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
            npcs[i].isTalking = false;
        }
    }

    public void OpenShop(ShopUI shopUI)
    {
        Player player = Player.Instance;
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
            shopUI = linkedShopUI;
        }

        CloseDialogue();
        if (shopUI != null)
        {
            shopUI.ShowShop();
        }
    }

    private List<string> GetActiveList(Player player, out bool allEnemiesDefeated)
    {
        allEnemiesDefeated = false;
        if (player == null)
        {
            return defaultDialogues;
        }

        player.hasKilledMonster = player.AreAllEnemiesDefeated();
        allEnemiesDefeated = player.hasKilledMonster;

        if (allEnemiesDefeated && hasKilledDialogues.Count > 0)
        {
            return hasKilledDialogues;
        }
        if (!allEnemiesDefeated && noKillDialogues.Count > 0)
        {
            return noKillDialogues;
        }
        return defaultDialogues;
    }
}
