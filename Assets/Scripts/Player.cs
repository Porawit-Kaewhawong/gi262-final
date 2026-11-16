using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player Ninja character that moves in 2D, throws shurikens in front of them,
/// collects Coins/XP, levels up, interacts with world objects, and respawns at Checkpoints.
/// Inherits from Character.
/// </summary>
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
    [SerializeField] private TextMeshProUGUI coinCurrencyText = null;
    [SerializeField] private TextMeshProUGUI levelCurrencyText = null;

    private int facingDirection = 1;
    private float moveInputX = 0f;
    private bool canDoubleJump = false;
    private float lastThrowTime = -10f;
    private float lastCombatTime = -10f;
    private float defaultXpGain = 25f;
    private Coroutine respawnCoroutine;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (string.IsNullOrWhiteSpace(Name) || Name == "Entity")
        {
            Name = "Ninja";
        }
        if (maxHealth <= 0f) maxHealth = 100f;
        if (moveSpeed <= 0.1f) moveSpeed = 7.5f;

        base.Awake();

        if (rb != null)
        {
            rb.gravityScale = 2.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Canvas[] childCanvases = GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < childCanvases.Length; i++)
        {
            if (childCanvases[i] != null)
            {
                childCanvases[i].gameObject.SetActive(false);
            }
        }

        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null) lr.enabled = false;
        SpringJoint2D sj = GetComponent<SpringJoint2D>();
        if (sj != null) sj.enabled = false;

        checkPointPos = transform.position;
    }

    private void Start()
    {
        SnapToInitialSpawnPoint();
        BindCameraToPlayer();
        BindCurrencyUI();
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
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckGroundCollision(groundLayer);
        if (isGrounded && rb != null && rb.linearVelocity.y <= 0.2f)
        {
            canDoubleJump = true;
        }

        if (isStunned)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        ReadPlayerInput();
    }

    private void FixedUpdate()
    {
        if (isDead || isStunned) return;
        Move();
    }

    private void ReadPlayerInput()
    {
        moveInputX = 0f;
        bool jumpPressed = false;
        bool throwPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInputX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInputX += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpPressed = true;
            }

            if (Keyboard.current.jKey.wasPressedThisFrame ||
                Keyboard.current.kKey.wasPressedThisFrame ||
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                throwPressed = true;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ApplyStatusEffect(new StatEffect("Speed Boost", 5f, 1.4f, false));
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ApplyStatusEffect(new DOTEffect("Poison DOT", 4f, 5f, 1f, true));
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ApplyStatusEffect(new StunEffect("Stun", 1.2f));
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null ||
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                throwPressed = true;
            }
        }

        if (Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.15f) moveInputX = Mathf.Sign(stickX);
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressed = true;
            if (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame) throwPressed = true;
        }

        if (Mathf.Abs(moveInputX) > 0.05f)
        {
            facingDirection = moveInputX > 0f ? 1 : -1;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingDirection < 0;
            }
        }

        if (jumpPressed && rb != null)
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

        if (throwPressed && Time.time >= lastThrowTime + shurikenCooldown)
        {
            ThrowShurikenInFront();
        }
    }

    public override void Move()
    {
        if (rb == null || isDead || isStunned) return;

        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(moveInputX) > 0.05f && isGrounded)
        {
            StepClimbIfLowObstacle(moveInputX);
        }
    }

    private void ThrowShurikenInFront()
    {
        if (shurikenPrefab == null) return;

        lastThrowTime = Time.time;
        isInCombat = true;
        lastCombatTime = Time.time;

        Vector2 throwDir = new Vector2(facingDirection, 0f);
        Vector3 spawnPos = transform.position + new Vector3(facingDirection * 0.7f, 0.1f, 0f);
        StartCoroutine(ShurikenProjectileRoutine(spawnPos, throwDir));
    }

    private IEnumerator ShurikenProjectileRoutine(Vector3 startPos, Vector2 dir)
    {
        GameObject proj = Instantiate(shurikenPrefab, startPos, Quaternion.identity);

        float elapsed = 0f;
        const float maxLifetime = 2.5f;
        int wallMask = groundLayer.value != 0 ? groundLayer.value : (1 << 6);

        while (elapsed < maxLifetime && proj != null)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            proj.transform.position += (Vector3)(dir * (shurikenSpeed * dt));
            proj.transform.Rotate(0f, 0f, -720f * Mathf.Sign(dir.x) * dt);

            Collider2D[] hits = Physics2D.OverlapCircleAll(proj.transform.position, 0.28f);
            bool hitSomething = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform.IsChildOf(transform) || hit.gameObject == gameObject) continue;

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.isDead)
                {
                    Attack(enemy);
                    hitSomething = true;
                    break;
                }

                Chest chest = hit.GetComponentInParent<Chest>();
                if (chest != null && !chest.isOpened)
                {
                    chest.Interact(this);
                    hitSomething = true;
                    break;
                }

                if (!hit.isTrigger && (((1 << hit.gameObject.layer) & wallMask) != 0 || hit.gameObject.layer == 6))
                {
                    hitSomething = true;
                    break;
                }
            }

            if (hitSomething)
            {
                break;
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
        if (enemy == null || enemy.isDead) return;

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
        if (coin == null) return;
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
        if (amount <= 0) return;
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
        if (WaveManager.Instance != null && WaveManager.Instance.waves != null && WaveManager.Instance.waves.Count > 0)
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
        defaultXpGain = expReward > 0 ? expReward : 25f;
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
        if (isDead || amount <= 0f) return;

        isInCombat = true;
        lastCombatTime = Time.time;

        base.TakeDamage(amount);

        if (currentHealth <= 0f && isDead)
        {
            if (respawnCoroutine != null) StopCoroutine(respawnCoroutine);
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

    private void StepClimbIfLowObstacle(float dirX)
    {
        if (col == null || rb == null || rb.linearVelocity.y > 0.2f) return;

        Bounds b = col.bounds;
        int mask = groundLayer.value != 0 ? groundLayer.value : (1 << 6);
        float sign = Mathf.Sign(dirX);
        Vector2 footOrigin = new Vector2(b.center.x, b.min.y + 0.08f);
        Vector2 waistOrigin = new Vector2(b.center.x, b.min.y + 0.65f);
        float dist = b.extents.x + 0.2f;

        RaycastHit2D footHit = Physics2D.Raycast(footOrigin, Vector2.right * sign, dist, mask);
        RaycastHit2D waistHit = Physics2D.Raycast(waistOrigin, Vector2.right * sign, dist, mask);

        if (footHit.collider != null && !footHit.collider.isTrigger && waistHit.collider == null)
        {
            transform.position += new Vector3(sign * 0.06f, 0.35f, 0f);
        }
    }

    private void SnapToInitialSpawnPoint()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.PlayerSpawnPoint != null)
        {
            transform.position = WaveManager.Instance.PlayerSpawnPoint.position;
        }
        checkPointPos = transform.position;
    }

    private void BindCameraToPlayer()
    {
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
        }
    }

    private void BindCurrencyUI()
    {
        if (coinCurrencyText == null)
        {
            GameObject coinObj = GameObject.Find("CoinCurrencyText");
            if (coinObj != null) coinCurrencyText = coinObj.GetComponent<TextMeshProUGUI>();
        }
        if (levelCurrencyText == null)
        {
            GameObject levelObj = GameObject.Find("LevelCurrencyText");
            if (levelObj != null) levelCurrencyText = levelObj.GetComponent<TextMeshProUGUI>();
        }
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
