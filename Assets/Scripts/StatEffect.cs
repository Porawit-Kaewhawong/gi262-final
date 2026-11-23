using System;
using UnityEngine;

[Serializable]
public class StatEffect : StatusEffect
{
    public float speedMultiplier = 1.4f;
    private float appliedSpeedDelta = 0f;

    public StatEffect()
    {
        effectName = "Speed Boost";
        duration = 5f;
        isDebuff = false;
    }

    public StatEffect(string name, float duration, float speedMultiplier, bool isDebuff = false)
        : base(name, duration, isDebuff)
    {
        this.speedMultiplier = speedMultiplier;
    }

    public override void OnApply(Character character)
    {
        base.OnApply(character);
        if (character == null)
        {
            return;
        }

        appliedSpeedDelta = character.moveSpeed * (speedMultiplier - 1f);
        character.moveSpeed = Mathf.Max(0.5f, character.moveSpeed + appliedSpeedDelta);
    }

    public override void OnUpdate(Character character)
    {
        base.OnUpdate(character);
    }

    public override void OnRemove(Character character)
    {
        base.OnRemove(character);
        if (character == null)
        {
            return;
        }

        character.moveSpeed = Mathf.Max(0.5f, character.moveSpeed - appliedSpeedDelta);
        appliedSpeedDelta = 0f;
    }
}
