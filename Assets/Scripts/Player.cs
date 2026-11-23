using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : Character
{
    public static Player Instance { get; private set; }

    [Header("Player Progression & Currency")]
    public int coinCurrency = 20;
    public int level = 1;
    public float currentXP = 0f;
    public float nextLevelXP = 50f;
    public float levelMultiplierXP = 1.5f;
    public float defaultXpGain = 25f;
    public int totalKills = 0;
    public bool hasKilledMonster = false;

    [Header("Checkpoint & Combat State")]
    public Vector2 checkPointPos;
    public bool isInCombat = false;
    public float respawnTime = 2.0f;

    [Header("Ninja Movement & Shuriken Settings")]
    public float jumpForce = 14.5f;
    public float attackDamage = 25f;
    public float shurikenSpeed = 16f;
    public float shurikenCooldown = 0.25f;
    public GameObject shurikenPrefab;
    public LayerMask groundLayer;

    [Header("Shuriken Status Effect Upgrades")]
    public bool enablePoisonShuriken = false;
    public bool enableStunShuriken = false;

    [Header("Currency UI References")]
    public TextMeshProUGUI coinCurrencyText;
    public TextMeshProUGUI levelCurrencyText;

    private int facingDirection = 1;
    private Vector2 inputValue;
    private bool canDoubleJump = false;
    private float lastThrowTime = -10f;
    private float lastCombatTime = -10f;
    private Coroutine respawnCoroutine;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        base.Awake();
        checkPointPos = transform.position;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    private void Start()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.playerSpawnPoint != null)
        {
            transform.position = WaveManager.Instance.playerSpawnPoint.position;
            checkPointPos = transform.position;
        }

        UpdateCurrencyUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        TickStatusEffects();

        if (isInCombat && Time.time > lastCombatTime + 4.0f)
        {
            isInCombat = false;
        }

        UpdateCurrencyUI();

        if (isDead)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        CheckGroundCollision(groundLayer);
        if (isGrounded && rb != null && rb.linearVelocity.y <= 0.2f)
        {
            canDoubleJump = true;
        }

        if (isStunned)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        ReadPlayerInput();
    }

    private void FixedUpdate()
    {
        if (isDead || isStunned)
        {
            return;
        }

        if (moveAction != null)
        {
            inputValue = moveAction.ReadValue<Vector2>();
        }
        Move();
    }

    private void ReadPlayerInput()
    {
        if (moveAction != null)
        {
            inputValue = moveAction.ReadValue<Vector2>();
            if (inputValue.x != 0f)
            {
                if (inputValue.x > 0f)
                {
                    facingDirection = 1;
                }
                else
                {
                    facingDirection = -1;
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = facingDirection < 0;
                }
            }
        }

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && rb != null)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
            }
            else if (canDoubleJump)
            {
                canDoubleJump = false;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.9f);
            }
        }

        if (attackAction != null && attackAction.WasPressedThisFrame() && Time.time >= lastThrowTime + shurikenCooldown)
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                ThrowShuriken();
            }
        }
    }

    public override void Move()
    {
        if (rb == null || isDead || isStunned)
        {
            return;
        }
        rb.linearVelocity = new Vector2(inputValue.x * moveSpeed, rb.linearVelocity.y);
    }

    private void ThrowShuriken()
    {
        if (shurikenPrefab == null)
        {
            return;
        }

        lastThrowTime = Time.time;
        isInCombat = true;
        lastCombatTime = Time.time;

        Vector3 spawnPos = transform.position + new Vector3(facingDirection * 0.7f, 0.1f, 0f);
        StartCoroutine(ShurikenProjectileRoutine(spawnPos, new Vector2(facingDirection, 0f)));
    }

    private IEnumerator ShurikenProjectileRoutine(Vector3 startPos, Vector2 dir)
    {
        GameObject proj = Instantiate(shurikenPrefab, startPos, Quaternion.identity);
        float elapsed = 0f;

        while (elapsed < 2.5f && proj != null)
        {
            elapsed += Time.deltaTime;
            proj.transform.position += (Vector3)(dir * shurikenSpeed * Time.deltaTime);
            proj.transform.Rotate(0f, 0f, -720f * Mathf.Sign(dir.x) * Time.deltaTime);

            Collider2D[] hits = Physics2D.OverlapCircleAll(proj.transform.position, 0.28f);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.gameObject == gameObject)
                {
                    continue;
                }

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.isDead)
                {
                    Attack(enemy);
                    Destroy(proj);
                    yield break;
                }

                if (!hit.isTrigger && (((1 << hit.gameObject.layer) & groundLayer.value) != 0))
                {
                    Destroy(proj);
                    yield break;
                }
            }

            yield return null;
        }

        if (proj != null)
        {
            Destroy(proj);
        }
    }

    public void Attack(Enemy enemy)
    {
        if (enemy == null || enemy.isDead)
        {
            return;
        }

        isInCombat = true;
        lastCombatTime = Time.time;
        enemy.TakeDamage(attackDamage);

        if (!enemy.isDead)
        {
            if (enablePoisonShuriken)
            {
                enemy.ApplyStatusEffect(new DOTEffect("Venom Shuriken", 4f, 6f, 1f, true));
            }
            if (enableStunShuriken)
            {
                enemy.ApplyStatusEffect(new StunEffect("Thunder Stun", 1.2f));
            }
        }
    }

    public void AddCoins(Coin coin)
    {
        if (coin == null)
        {
            return;
        }
        coinCurrency += Mathf.Max(0, coin.coinDrop);
        UpdateCurrencyUI();
    }

    public void SetCheckPoint(Vector2 newPos)
    {
        checkPointPos = newPos;
    }

    public void AddXP()
    {
        currentXP += defaultXpGain;
        while (currentXP >= nextLevelXP && nextLevelXP > 0f)
        {
            LevelUp();
        }
        UpdateCurrencyUI();
    }

    public void LevelUp()
    {
        currentXP = Mathf.Max(0f, currentXP - nextLevelXP);
        level++;
        nextLevelXP = Mathf.Max(10f, nextLevelXP * Mathf.Max(1.1f, levelMultiplierXP));

        maxHealth += 15f;
        currentHealth = maxHealth;
        attackDamage += 4f;
    }

    public void DeductLevel(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        level = Mathf.Max(1, level - amount);
        UpdateCurrencyUI();
    }

    public void OnEnemyKilled()
    {
        totalKills++;
        AddXP();
        hasKilledMonster = AreAllEnemiesDefeated();
    }

    public bool AreAllEnemiesDefeated()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.waves.Count > 0)
        {
            if (WaveManager.Instance.isWaveInProgress || WaveManager.Instance.currentWaveIndex < WaveManager.Instance.waves.Count)
            {
                return false;
            }
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && !enemies[i].isDead)
            {
                return false;
            }
        }

        return totalKills > 0;
    }

    public void GrantEnemyKillReward(int expReward)
    {
        if (expReward > 0)
        {
            defaultXpGain = expReward;
        }
        else
        {
            defaultXpGain = 25f;
        }
        OnEnemyKilled();
    }

    public void RespawnAtCheckpoint()
    {
        ClearAllStatusEffects();

        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        Checkpoint activeCp = null;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null && checkpoints[i].isActivated)
            {
                activeCp = checkpoints[i];
                break;
            }
        }

        if (activeCp != null)
        {
            activeCp.RestorePlayer(this);
        }
        else
        {
            ApplyPenalty(currentXP * 0.1f, 5);
            transform.position = checkPointPos;
            currentHealth = maxHealth;
            isDead = false;
            isInCombat = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public override void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        isInCombat = true;
        lastCombatTime = Time.time;
        base.TakeDamage(amount);

        if (currentHealth <= 0f && isDead)
        {
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }
            respawnCoroutine = StartCoroutine(RespawnTimerRoutine());
        }
    }

    public void ApplyPenalty(float xpLoss, int coinLoss)
    {
        currentXP = Mathf.Max(0f, currentXP - Mathf.Max(0f, xpLoss));
        coinCurrency = Mathf.Max(0, coinCurrency - Mathf.Max(0, coinLoss));
        UpdateCurrencyUI();
    }

    private IEnumerator RespawnTimerRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0.5f, respawnTime));
        RespawnAtCheckpoint();
        respawnCoroutine = null;
    }

    private void UpdateCurrencyUI()
    {
        if (coinCurrencyText != null)
        {
            coinCurrencyText.text = $"Coins: {coinCurrency}";
        }
        if (levelCurrencyText != null)
        {
            levelCurrencyText.text = $"Level: {level}";
        }
    }
}
