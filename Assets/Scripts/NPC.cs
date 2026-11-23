using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC Settings")]
    public DialogueUI dialogueUI;
    public bool isTalking = false;

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null || dialogueUI == null)
        {
            return;
        }

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
