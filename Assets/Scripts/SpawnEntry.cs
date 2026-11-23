using System;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
    public string displayName = "Ninja Enemy";
    public GameObject enemyAsset;
    public int amountPerWave = 3;
    public float spawnInterval = 0.6f;
}
