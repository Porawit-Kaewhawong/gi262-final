using UnityEngine;

/// <summary>
/// Chest interactable that opens and spawns an item reward prefab when interacted with (or hit by a shuriken).
/// Inherits from Interactable.
/// </summary>
public class Chest : Interactable
{
    [Header("Chest Settings")]
    public bool isOpened = false;
    public GameObject itemReward;

    protected override void Awake()
    {
        Name = "Chest";
        isInteractable = !isOpened;
        isOneTimeUse = true;
        if (interactRange <= 0.1f) interactRange = 2.0f;
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (isOpened || !isInteractable) return;

        isOpened = true;
        base.Interact(player);

        if (itemReward != null)
        {
            Vector3 dropPos = transform.position + Vector3.up * 0.6f;
            Instantiate(itemReward, dropPos, Quaternion.identity);
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = new Color(0.45f, 0.45f, 0.45f, 0.45f);
            }
        }
    }
}
