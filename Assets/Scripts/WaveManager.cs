using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detailed Wave System manager that spawns waves of enemy prefabs and tracks wave completion.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave System")]
    public List<Wave> waves = new List<Wave>();
    public int currentWaveIndex = 0;
    public bool isWaveInProgress = false;
    public Transform[] spawnPoints;

    [HideInInspector] [SerializeField] private Transform playerSpawnPoint = null;
    [HideInInspector] [SerializeField] private GameObject portalPrefab = null;
    [HideInInspector] [SerializeField] private GameObject portalSceneObject = null;
    [HideInInspector] [SerializeField] private Transform portalSpawnPoint = null;
    [HideInInspector] [SerializeField] private LayerMask groundLayer;

    private int remainingEnemiesInWave = 0;
    private Coroutine activeWaveCoroutine;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public int RemainingEnemies => remainingEnemiesInWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (portalSceneObject != null)
        {
            portalSceneObject.SetActive(false);
        }
    }

    private void Start()
    {
        currentWaveIndex = 0;
        if (waves != null && waves.Count > 0)
        {
            StartNextWave();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartNextWave()
    {
        if (waves == null || currentWaveIndex >= waves.Count)
        {
            isWaveInProgress = false;
            SpawnStageCompletionPortal();
            return;
        }

        if (activeWaveCoroutine != null)
        {
            StopCoroutine(activeWaveCoroutine);
        }
        activeWaveCoroutine = StartCoroutine(RunWaveRoutine(waves[currentWaveIndex]));
    }

    public void OnEnemyDefeated()
    {
        if (!isWaveInProgress) return;

        remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);
        if (remainingEnemiesInWave <= 0)
        {
            OnWaveCompleted();
        }
    }

    public void OnWaveCompleted()
    {
        isWaveInProgress = false;
        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            StartNextWave();
        }
        else
        {
            Debug.Log("[WaveManager] All waves completed!");
            if (Player.Instance != null)
            {
                Player.Instance.hasKilledMonster = Player.Instance.AreAllEnemiesDefeated();
            }
            SpawnStageCompletionPortal();
        }
    }

    private IEnumerator RunWaveRoutine(Wave wave)
    {
        if (wave == null)
        {
            OnWaveCompleted();
            yield break;
        }

        int prePlacedCount = 0;
        if (currentWaveIndex == 0)
        {
            Enemy[] existing = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null && !existing[i].isDead)
                {
                    prePlacedCount++;
                }
            }
        }

        isWaveInProgress = true;
        remainingEnemiesInWave = Mathf.Max(1, wave.getTotalEnemyCount() + prePlacedCount);

        if (wave.waveDelay > 0f)
        {
            yield return new WaitForSeconds(wave.waveDelay);
        }

        int spawnedCount = 0;
        if (wave.spawnEntries != null)
        {
            for (int e = 0; e < wave.spawnEntries.Count; e++)
            {
                SpawnEntry entry = wave.spawnEntries[e];
                if (entry == null || entry.amountPerWave <= 0 || entry.enemyAsset == null) continue;

                float interval = Mathf.Max(0.15f, entry.spawnInterval);
                for (int i = 0; i < entry.amountPerWave; i++)
                {
                    Vector3 spawnPos = ChooseSpawnPosition(spawnedCount);
                    Instantiate(entry.enemyAsset, spawnPos, Quaternion.identity);
                    spawnedCount++;
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        if (spawnedCount == 0 && prePlacedCount == 0)
        {
            remainingEnemiesInWave = 0;
            OnWaveCompleted();
        }
    }

    private Vector3 ChooseSpawnPosition(int index)
    {
        Player player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
        Vector2 origin = player != null
            ? (Vector2)player.transform.position
            : (playerSpawnPoint != null ? (Vector2)playerSpawnPoint.position : Vector2.zero);

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform pt = spawnPoints[index % spawnPoints.Length];
            if (pt != null)
            {
                return FindGroundBelow(pt.position, origin);
            }
        }

        float dir = (index % 2 == 0) ? 1f : -1f;
        float dist = Random.Range(4.5f, 8.5f);
        Vector2 candidate = origin + new Vector2(dir * dist, 2.5f);
        return FindGroundBelow(candidate, origin);
    }

    private Vector3 FindGroundBelow(Vector2 candidate, Vector2 fallbackOrigin)
    {
        int mask = groundLayer.value != 0 ? groundLayer.value : (1 << 6) | 1;
        for (int attempt = 0; attempt < 6; attempt++)
        {
            float offsetX = attempt == 0 ? 0f : ((attempt % 2 == 0 ? 1f : -1f) * attempt * 1.5f);
            Vector2 rayStart = new Vector2(candidate.x + offsetX, fallbackOrigin.y + 4f);
            if (Physics2D.OverlapPoint(rayStart, mask) != null)
            {
                rayStart.y = fallbackOrigin.y + 1.5f;
            }

            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 16f, mask);
            if (hit.collider != null && hit.normal.y > 0.6f)
            {
                return new Vector3(hit.point.x, hit.point.y + 0.6f, 0f);
            }
        }

        return new Vector3(fallbackOrigin.x + 4f, fallbackOrigin.y + 0.5f, 0f);
    }

    private void SpawnStageCompletionPortal()
    {
        if (portalSceneObject != null)
        {
            if (portalSpawnPoint != null)
            {
                portalSceneObject.transform.position = portalSpawnPoint.position;
            }
            portalSceneObject.SetActive(true);
            return;
        }

        Portal existingPortal = FindFirstObjectByType<Portal>();
        if (existingPortal != null)
        {
            existingPortal.gameObject.SetActive(true);
            return;
        }

        if (portalPrefab != null)
        {
            Vector3 pos = portalSpawnPoint != null
                ? portalSpawnPoint.position
                : (Player.Instance != null ? Player.Instance.transform.position + Vector3.right * 3.5f : Vector3.zero);
            Instantiate(portalPrefab, pos, Quaternion.identity);
        }
    }
}
