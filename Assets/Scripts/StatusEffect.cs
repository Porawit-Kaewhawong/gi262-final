using System;
using UnityEngine;

[Serializable]
public class StatusEffect
{
    public string effectName = "Status Effect";
    public float duration = 3f;
    public float timer = 0f;
    public bool isDebuff = false;

    public StatusEffect() { }

    public StatusEffect(string name, float duration, bool isDebuff)
    {
        this.effectName = name;
        this.duration = duration;
        this.timer = 0f;
        this.isDebuff = isDebuff;
    }

    public virtual void OnApply(Character character)
    {
        timer = 0f;
    }

    public virtual void OnUpdate(Character character)
    {
        timer += Time.deltaTime;
    }

    public virtual void OnRemove(Character character)
    {
    }
}
