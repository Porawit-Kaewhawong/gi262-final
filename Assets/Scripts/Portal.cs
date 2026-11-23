using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : Interactable
{
    [Header("Portal Settings")]
    public Transform destination;
    public bool isTeleported = false;

    public override void Interact(Player player)
    {
        if (!isInteractable || player == null)
        {
            return;
        }

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
            SceneManager.LoadScene((currentIndex + 1) % totalScenes);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
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
}
