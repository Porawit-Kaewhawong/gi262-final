using System;
using UnityEngine;

/// <summary>
/// Configuration entry for a single enemy type within a Wave.
/// </summary>
[Serializable]
public class SpawnEntry
{
    public string displayName = "Ninja Enemy";
    public GameObject enemyAsset;
    public int amountPerWave = 3;
    public float spawnInterval = 0.6f;
}
