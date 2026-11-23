using UnityEngine;

public class Enemy : Character
{
    [Header("Enemy Rewards & Drops")]
    public GameObject[] DropableItems;
    public int expReward = 25;

    [Header("Enemy Combat & AI")]
    public float contactDamage = 15f;
    public float attackRange = 1.15f;
    public float attackCooldown = 1.0f;
    public float jumpForce = 9.5f;
    public LayerMask groundLayer;

    private float lastAttackTime = -10f;
    private float nextJumpTime = 0f;

    private void Update()
    {
        if (isDead)
        {
            return;
        }

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
            float dist = Vector2.Distance(player.transform.position, transform.position);
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(player);
            }
        }
    }

    public void DropItem(int amount)
    {
        if (DropableItems == null || DropableItems.Length == 0)
        {
            return;
        }

        for (int i = 0; i < Mathf.Max(1, amount); i++)
        {
            GameObject chosen = DropableItems[Random.Range(0, DropableItems.Length)];
            if (chosen == null)
            {
                continue;
            }

            Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), 0.35f, 0f);
            Instantiate(chosen, transform.position + offset, Quaternion.identity);
        }
    }

    public void Attack(Player player)
    {
        if (isDead || isStunned || player == null || player.isDead)
        {
            return;
        }
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        player.TakeDamage(contactDamage);
    }

    public override void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        base.TakeDamage(amount);

        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }

    public override void Move()
    {
        if (isDead || isStunned || rb == null)
        {
            return;
        }

        Player player = Player.Instance;
        if (player == null || player.isDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float diffX = player.transform.position.x - transform.position.x;
        if (Mathf.Abs(diffX) < 0.25f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dirX = Mathf.Sign(diffX);
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dirX < 0f;
        }

        if (isGrounded && Time.time >= nextJumpTime)
        {
            Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, 0.85f, groundLayer);
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

        if (Player.Instance != null)
        {
            Player.Instance.GrantEnemyKillReward(expReward);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead || collision == null)
        {
            return;
        }
        Player player = collision.collider.GetComponentInParent<Player>();
        if (player != null)
        {
            Attack(player);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || other == null)
        {
            return;
        }
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Attack(player);
        }
    }
}
