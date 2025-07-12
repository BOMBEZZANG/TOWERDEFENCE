using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Wave Data", menuName = "Tower Defense/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    public int waveNumber;
    public float spawnDelay = 0.5f;
    public float timeBetweenWaves = 5f;
    
    [Header("Enemy Spawns")]
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
}

[System.Serializable]
public class EnemySpawn
{
    public EnemyData enemyData;
    public int count;
    public float spawnDelay = 0.5f;
}