using UnityEngine;

/// <summary>
/// Coin pickup interactable that grants coin currency to the Player.
/// Inherits from Interactable.
/// </summary>
public class Coin : Interactable
{
    [Header("Coin Settings")]
    public int coinDrop = 10;

    protected override bool AutoInteractInRange => true;

    protected override void Awake()
    {
        Name = "Coin";
        isInteractable = true;
        isOneTimeUse = true;
        if (interactRange <= 0.1f) interactRange = 1.2f;
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null) return;

        base.Interact(player);
        player.AddCoins(this);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable || other == null) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Interact(player);
        }
    }
}
