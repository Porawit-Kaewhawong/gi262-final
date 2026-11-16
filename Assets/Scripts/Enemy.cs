using UnityEngine;

/// <summary>
/// Enemy entity that moves toward the Player, attacks on contact/range, takes damage, drops items, and grants XP on death.
/// Inherits from Character.
/// </summary>
public class Enemy : Character
{
    [Header("Enemy Rewards & Drops")]
    public GameObject[] DropableItems;
    public int expReward = 25;

    [Header("Enemy Combat & AI")]
    [SerializeField] private float contactDamage = 15f;
    [SerializeField] private float attackRange = 1.15f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float jumpForce = 9.5f;
    [SerializeField] private LayerMask groundLayer;

    private float lastAttackTime = -10f;
    private float nextJumpTime = 0f;

    protected override void Awake()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name == "Entity")
        {
            Name = gameObject.name.Replace("(Clone)", "").Trim();
        }

        if (Name.Contains("Samurai") || Name.Contains("Boss"))
        {
            maxHealth = 150f;
            expReward = 100;
            moveSpeed = 3.2f;
        }
        else if (Name.Contains("Armored"))
        {
            maxHealth = 75f;
            expReward = 45;
            moveSpeed = 2.2f;
        }
        else
        {
            if (maxHealth <= 1f || maxHealth == 100f) maxHealth = 50f;
            if (expReward <= 0) expReward = 25;
            if (moveSpeed <= 0.1f) moveSpeed = 2.6f;
        }

        base.Awake();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if (isDead) return;

        TickStatusEffects();
        CheckGroundCollision(groundLayer);

        if (isStunned)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        Move();

        Player player = Player.Instance;
        if (player != null && !player.isDead)
        {
            float distSq = ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distSq <= attackRange * attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(player);
            }
        }
    }

    public void DropItem(int amount)
    {
        if (DropableItems == null || DropableItems.Length == 0) return;

        int count = Mathf.Max(1, amount);
        for (int i = 0; i < count; i++)
        {
            GameObject chosen = DropableItems[Random.Range(0, DropableItems.Length)];
            if (chosen == null) continue;

            Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), 0.35f, 0f);
            Instantiate(chosen, transform.position + offset, Quaternion.identity);
        }
    }

    public void Attack(Player player)
    {
        if (isDead || isStunned || player == null || player.isDead) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        player.TakeDamage(contactDamage);
    }

    public override void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        base.TakeDamage(amount);

        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }

    public override void Move()
    {
        if (isDead || isStunned || rb == null) return;

        Player player = Player.Instance;
        if (player == null || player.isDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        if (Mathf.Abs(toPlayer.x) < 0.25f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dirX = Mathf.Sign(toPlayer.x);
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dirX < 0f;
        }

        if (isGrounded && Time.time >= nextJumpTime)
        {
            int mask = groundLayer.value != 0 ? groundLayer.value : (1 << 6);
            Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, 0.85f, mask);
            if (wallHit.collider != null && !wallHit.collider.isTrigger)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                nextJumpTime = Time.time + 0.9f;
            }
        }
    }

    public void OnDeath()
    {
        isDead = true;
        ClearAllStatusEffects();

        DropItem(1);

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated();
        }

        Player player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.GrantEnemyKillReward(expReward);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead || collision == null) return;
        Player player = collision.collider.GetComponentInParent<Player>();
        if (player != null)
        {
            Attack(player);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || other == null) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Attack(player);
        }
    }
}
