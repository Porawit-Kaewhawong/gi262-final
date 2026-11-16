using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single wave of enemies managed by WaveManager.
/// </summary>
[Serializable]
public class Wave
{
    public int waveNumber = 1;
    public float waveDelay = 2f;
    public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

    public int getTotalEnemyCount()
    {
        if (spawnEntries == null) return 0;

        int total = 0;
        for (int i = 0; i < spawnEntries.Count; i++)
        {
            if (spawnEntries[i] != null && spawnEntries[i].amountPerWave > 0)
            {
                total += spawnEntries[i].amountPerWave;
            }
        }
        return total;
    }
}
