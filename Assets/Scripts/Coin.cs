using UnityEngine;

public class Coin : Interactable
{
    [Header("Coin Settings")]
    public int coinDrop = 10;

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null)
        {
            return;
        }

        base.Interact(player);
        player.AddCoins(this);
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
