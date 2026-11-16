using UnityEngine;

/// <summary>
/// Checkpoint interactable that saves the Player's respawn point and restores the Player with XP/Coin penalty.
/// Inherits from Interactable.
/// </summary>
public class Checkpoint : Interactable
{
    [Header("Checkpoint Settings")]
    public string checkPointID = "CP_01";
    public bool isActivated = false;
    public float expPenaltyPercent = 0.15f;
    public int coinPenaltyAmount = 5;

    private SpriteRenderer flagRenderer;

    protected override void Awake()
    {
        Name = "Checkpoint";
        isInteractable = true;
        isOneTimeUse = false;
        if (interactRange <= 0.1f) interactRange = 2.0f;
        base.Awake();
        flagRenderer = GetComponentInChildren<SpriteRenderer>();
        UpdateVisualState();
    }

    public override void Interact(Player player)
    {
        if (player == null) return;

        isActivated = true;
        player.SetCheckPoint(transform.position);
        UpdateVisualState();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null && !player.isDead)
        {
            Interact(player);
        }
    }

    public void RestorePlayer(Player player)
    {
        if (player == null) return;

        float penaltyRatio = expPenaltyPercent > 1f ? (expPenaltyPercent / 100f) : Mathf.Clamp01(expPenaltyPercent);
        float xpLoss = player.currentXP * penaltyRatio;
        player.ApplyPenalty(xpLoss, coinPenaltyAmount);

        player.transform.position = transform.position;
        player.currentHealth = player.maxHealth;
        player.isDead = false;
        player.isInCombat = false;
    }

    private void UpdateVisualState()
    {
        if (flagRenderer == null) flagRenderer = GetComponentInChildren<SpriteRenderer>();
        if (flagRenderer != null)
        {
            flagRenderer.color = isActivated
                ? new Color(0.25f, 1f, 0.45f, 1f)
                : new Color(0.85f, 0.85f, 0.9f, 1f);
        }
    }
}
