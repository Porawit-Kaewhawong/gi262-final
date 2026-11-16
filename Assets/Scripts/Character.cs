using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all living characters (Player and Enemy).
/// Inherits from Identity.
/// </summary>
public class Character : Identity
{
    [Header("Character Stats")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public bool isGrounded = false;
    public bool isDead = false;

    [Header("Status Effects")]
    [HideInInspector] public bool isStunned = false;
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;
    protected Color baseSpriteColor = Color.white;
    private Coroutine hitFlashRoutine;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseSpriteColor = spriteRenderer.color;
        }

        Animator[] animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].enabled = false;
            }
        }

        if (maxHealth <= 0f) maxHealth = 100f;
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        TriggerHitFlash();

        if (currentHealth <= 0f)
        {
            isDead = true;
        }
    }

    public virtual void Move()
    {
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null || isDead) return;

        // Refresh existing effect of the same name if already present
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null && activeEffects[i].effectName == effect.effectName)
            {
                activeEffects[i].timer = 0f;
                activeEffects[i].duration = Mathf.Max(activeEffects[i].duration, effect.duration);
                return;
            }
        }

        activeEffects.Add(effect);
        effect.OnApply(this);
    }

    protected void TickStatusEffects()
    {
        SyncGridPosition();

        if (activeEffects.Count == 0) return;

        bool anyStun = false;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            if (effect == null)
            {
                activeEffects.RemoveAt(i);
                continue;
            }

            effect.OnUpdate(this);
            if (effect is StunEffect)
            {
                anyStun = true;
            }

            if (effect.timer >= effect.duration)
            {
                effect.OnRemove(this);
                activeEffects.RemoveAt(i);
            }
        }

        if (!anyStun && isStunned)
        {
            isStunned = false;
        }
    }

    protected void ClearAllStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].OnRemove(this);
            }
        }
        activeEffects.Clear();
        isStunned = false;
    }

    protected bool CheckGroundCollision(LayerMask groundMask)
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return false;

        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
        Vector2 size = new Vector2(Mathf.Max(0.2f, b.size.x * 0.75f), 0.12f);
        int mask = groundMask.value != 0 ? groundMask.value : (1 << 6) | 1;
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.18f, mask);
        isGrounded = hit.collider != null && hit.collider != col && !hit.collider.isTrigger;
        return isGrounded;
    }

    protected void TriggerHitFlash()
    {
        if (spriteRenderer == null || !gameObject.activeInHierarchy) return;
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlashCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
        }
        yield return new WaitForSeconds(0.12f);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseSpriteColor;
        }
        hitFlashRoutine = null;
    }
}
