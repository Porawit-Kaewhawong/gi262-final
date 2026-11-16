using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for all interactable objects in the world.
/// Inherits from Identity.
/// </summary>
public class Interactable : Identity
{
    [Header("Interactable Settings")]
    public bool isInteractable = true;
    public bool isOneTimeUse = true;
    public float interactRange = 2.0f;

    protected virtual bool AutoInteractInRange => false;

    public virtual void Interact(Player player)
    {
        if (!isInteractable || player == null) return;

        if (isOneTimeUse)
        {
            isInteractable = false;
        }
    }

    private void Update()
    {
        SyncGridPosition();

        Player player = Player.Instance;
        if (!isInteractable || player == null || player.isDead)
        {
            return;
        }

        float range = Mathf.Max(0.5f, interactRange);
        float distSq = ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude;

        if (distSq <= range * range)
        {
            if (AutoInteractInRange)
            {
                Interact(player);
                return;
            }

            bool interactPressed = false;
            if (Keyboard.current != null)
            {
                interactPressed = Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame;
            }
            if (!interactPressed && Gamepad.current != null)
            {
                interactPressed = Gamepad.current.buttonNorth.wasPressedThisFrame;
            }

            if (interactPressed)
            {
                Interact(player);
            }
        }
    }
}
