using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave System")]
    public List<Wave> waves = new List<Wave>();
    public int currentWaveIndex = 0;
    public bool isWaveInProgress = false;
    public Transform[] spawnPoints;

    [Header("Stage References")]
    public Transform playerSpawnPoint;
    public GameObject portalPrefab;
    public GameObject portalSceneObject;
    public Transform portalSpawnPoint;
    public LayerMask groundLayer;

    private int remainingEnemiesInWave = 0;
    private Coroutine activeWaveCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentWaveIndex = 0;
        if (waves.Count > 0)
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
        if (currentWaveIndex >= waves.Count)
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
        if (!isWaveInProgress)
        {
            return;
        }

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
        for (int e = 0; e < wave.spawnEntries.Count; e++)
        {
            SpawnEntry entry = wave.spawnEntries[e];
            if (entry == null || entry.amountPerWave <= 0 || entry.enemyAsset == null)
            {
                continue;
            }

            for (int i = 0; i < entry.amountPerWave; i++)
            {
                Instantiate(entry.enemyAsset, ChooseSpawnPosition(spawnedCount), Quaternion.identity);
                spawnedCount++;
                yield return new WaitForSeconds(Mathf.Max(0.15f, entry.spawnInterval));
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
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform pt = spawnPoints[index % spawnPoints.Length];
            if (pt != null)
            {
                return pt.position;
            }
        }

        if (Player.Instance != null)
        {
            float dir = -1f;
            if (index % 2 == 0)
            {
                dir = 1f;
            }
            return Player.Instance.transform.position + new Vector3(dir * Random.Range(4.5f, 8.5f), 1f, 0f);
        }

        return Vector3.zero;
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

        Portal existingPortal = FindFirstObjectByType<Portal>(FindObjectsInactive.Include);
        if (existingPortal != null)
        {
            existingPortal.gameObject.SetActive(true);
            return;
        }

        if (portalPrefab != null)
        {
            Vector3 pos = Vector3.zero;
            if (portalSpawnPoint != null)
            {
                pos = portalSpawnPoint.position;
            }
            else if (Player.Instance != null)
            {
                pos = Player.Instance.transform.position + Vector3.right * 3.5f;
            }
            Instantiate(portalPrefab, pos, Quaternion.identity);
        }
    }
}
