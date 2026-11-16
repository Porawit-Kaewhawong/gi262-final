using UnityEngine;

/// <summary>
/// NPC interactable that triggers dynamic dialogue based on Player state and provides access to the ShopUI.
/// Inherits from Interactable.
/// </summary>
public class NPC : Interactable
{
    [Header("NPC Settings")]
    public DialogueUI dialogueUI;
    public bool isTalking = false;

    protected override void Awake()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name == "Entity")
        {
            Name = "NPC";
        }
        isInteractable = true;
        isOneTimeUse = false;
        if (interactRange <= 0.1f) interactRange = 2.5f;
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (player == null) return;

        if (dialogueUI == null)
        {
            dialogueUI = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        }
        if (dialogueUI == null) return;

        if (!isTalking)
        {
            isTalking = true;
            dialogueUI.StartDialogue(player);
        }
        else
        {
            dialogueUI.AdvanceOrClose(player, this);
        }
    }
}
