using UnityEngine;

public class Health : Interactable
{
    [Header("Health Pickup Settings")]
    public int healAmount = 25;

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null)
        {
            return;
        }

        base.Interact(player);
        player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + healAmount);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable || other == null)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Interact(player);
        }
    }
}
