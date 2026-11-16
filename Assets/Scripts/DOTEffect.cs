using System;
using UnityEngine;

/// <summary>
/// Damage Over Time (or Heal Over Time) status effect applied to a Character.
/// </summary>
[Serializable]
public class DOTEffect : StatusEffect
{
    public float damagePerTick = 5f;
    public float tickInterval = 1f;
    private float tickTimer = 0f;

    public DOTEffect()
    {
        effectName = "Poison DOT";
        duration = 4f;
        isDebuff = true;
    }

    public DOTEffect(string name, float duration, float damagePerTick, float tickInterval = 1f, bool isDebuff = true)
        : base(name, duration, isDebuff)
    {
        this.damagePerTick = damagePerTick;
        this.tickInterval = Mathf.Max(0.1f, tickInterval);
    }

    public override void OnApply(Character character)
    {
        base.OnApply(character);
        tickTimer = 0f;
    }

    public override void OnUpdate(Character character)
    {
        base.OnUpdate(character);
        if (character == null || character.isDead) return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            if (isDebuff)
            {
                character.TakeDamage(damagePerTick);
            }
            else
            {
                character.currentHealth = Mathf.Min(character.maxHealth, character.currentHealth + damagePerTick);
            }
        }
    }

    public override void OnRemove(Character character)
    {
        base.OnRemove(character);
    }
}
