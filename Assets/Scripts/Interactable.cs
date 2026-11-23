using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : Identity
{
    private static List<Interactable> allInteractables = new List<Interactable>();
    private static int lastInteractFrame = -1;

    [Header("Interactable Settings")]
    public bool isInteractable = true;
    public bool isOneTimeUse = true;
    public float interactRange = 2.0f;
    public bool autoInteract = false;

    protected virtual void OnEnable()
    {
        if (!allInteractables.Contains(this))
        {
            allInteractables.Add(this);
        }
    }

    protected virtual void OnDisable()
    {
        allInteractables.Remove(this);
    }

    public virtual void Interact(Player player)
    {
        if (!isInteractable || player == null)
        {
            return;
        }

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

        float dist = Vector2.Distance(player.transform.position, transform.position);
        if (dist > interactRange)
        {
            return;
        }

        if (autoInteract)
        {
            Interact(player);
            return;
        }

        if (lastInteractFrame == Time.frameCount)
        {
            return;
        }

        bool pressed = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            pressed = true;
        }
        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            pressed = true;
        }

        if (pressed && IsClosestManual(player))
        {
            lastInteractFrame = Time.frameCount;
            Interact(player);
        }
    }

    private bool IsClosestManual(Player player)
    {
        Interactable closest = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < allInteractables.Count; i++)
        {
            Interactable item = allInteractables[i];
            if (item == null || !item.isInteractable || item.autoInteract)
            {
                continue;
            }

            float d = Vector2.Distance(player.transform.position, item.transform.position);
            if (d <= item.interactRange && d < bestDist)
            {
                bestDist = d;
                closest = item;
            }
        }

        return closest == this;
    }
}
