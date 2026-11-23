using TMPro;
using UnityEngine;

public class Chest : Interactable
{
    [Header("Chest Settings")]
    public bool isOpened = false;
    public GameObject itemReward;

    public override void Interact(Player player)
    {
        if (isOpened || !isInteractable)
        {
            return;
        }

        isOpened = true;
        base.Interact(player);

        if (itemReward != null)
        {
            Instantiate(itemReward, transform.position + Vector3.up * 0.6f, Quaternion.identity);
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = new Color(0.45f, 0.45f, 0.45f, 0.45f);
        }

        TMP_Text label = GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "Opened";
            label.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        }

        enabled = false;
    }
}
