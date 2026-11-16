using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal interactable that teleports the Player to a destination Transform or reloads the stage.
/// Inherits from Interactable.
/// </summary>
public class Portal : Interactable
{
    [Header("Portal Settings")]
    public Transform destination;
    public bool isTeleported = false;

    protected override void Awake()
    {
        Name = "Portal";
        isInteractable = true;
        isOneTimeUse = false;
        if (interactRange <= 0.1f) interactRange = 2.0f;
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null) return;

        isTeleported = true;
        base.Interact(player);

        if (destination != null)
        {
            player.transform.position = destination.position;
            player.SetCheckPoint(destination.position);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        if (totalScenes > 0)
        {
            int nextIndex = (currentIndex + 1) % totalScenes;
            SceneManager.LoadScene(nextIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable || other == null) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null && !player.isDead)
        {
            Interact(player);
        }
    }
}
