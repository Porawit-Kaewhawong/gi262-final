using System;
using UnityEngine;

[Serializable]
public class StunEffect : StatusEffect
{
    public StunEffect()
    {
        effectName = "Stun";
        duration = 1.5f;
        isDebuff = true;
    }

    public StunEffect(string name, float duration)
        : base(name, duration, true)
    {
    }

    public override void OnApply(Character character)
    {
        base.OnApply(character);
        if (character == null)
        {
            return;
        }
        character.isStunned = true;
    }

    public override void OnUpdate(Character character)
    {
        base.OnUpdate(character);
        if (character != null)
        {
            character.isStunned = true;
        }
    }

    public override void OnRemove(Character character)
    {
        base.OnRemove(character);
        if (character == null)
        {
            return;
        }
        character.isStunned = false;
    }
}
