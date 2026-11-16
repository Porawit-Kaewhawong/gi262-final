using UnityEngine;

/// <summary>
/// Health pickup interactable that restores HP to the Player.
/// Inherits from Interactable.
/// </summary>
public class Health : Interactable
{
    [Header("Health Pickup Settings")]
    public int healAmount = 25;

    protected override bool AutoInteractInRange => true;

    protected override void Awake()
    {
        Name = "Health";
        isInteractable = true;
        isOneTimeUse = true;
        if (interactRange <= 0.1f) interactRange = 1.2f;
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null) return;

        base.Interact(player);
        player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + healAmount);
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
