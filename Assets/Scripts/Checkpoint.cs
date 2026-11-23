using UnityEngine;

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
        base.Awake();
        flagRenderer = GetComponentInChildren<SpriteRenderer>();
        UpdateVisualState();
    }

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null)
        {
            return;
        }

        Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            if (allCheckpoints[i] != null && allCheckpoints[i] != this && allCheckpoints[i].isActivated)
            {
                allCheckpoints[i].isActivated = false;
                allCheckpoints[i].UpdateVisualState();
            }
        }

        isActivated = true;
        player.SetCheckPoint(transform.position);
        UpdateVisualState();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable || other == null)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();
        if (player != null && !player.isDead)
        {
            Interact(player);
        }
    }

    public void RestorePlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        float xpLoss = player.currentXP * Mathf.Clamp01(expPenaltyPercent);
        player.ApplyPenalty(xpLoss, coinPenaltyAmount);

        player.transform.position = transform.position;
        player.currentHealth = player.maxHealth;
        player.isDead = false;
        player.isInCombat = false;
    }

    private void UpdateVisualState()
    {
        if (flagRenderer == null)
        {
            flagRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (flagRenderer != null)
        {
            if (isActivated)
            {
                flagRenderer.color = new Color(0.25f, 1f, 0.45f, 1f);
            }
            else
            {
                flagRenderer.color = new Color(0.85f, 0.85f, 0.9f, 1f);
            }
        }
    }
}
