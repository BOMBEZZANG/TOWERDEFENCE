using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameBalanceManager : MonoBehaviour
{
    public static GameBalanceManager Instance;
    
    [Header("Data Sources")]
    public List<EnemyData> enemyDataAssets;
    public List<TowerData> towerDataAssets;
    public List<WaveData> waveDataAssets;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Simple method to get current game data for display purposes
    public void LogCurrentGameData()
    {
        Debug.Log("=== Current Game Balance Data ===");
        
        Debug.Log("Enemy Data:");
        foreach (var enemy in enemyDataAssets)
        {
            Debug.Log($"- {enemy.name}: Health={enemy.health}, Speed={enemy.speed}, Reward={enemy.reward}");
        }
        
        Debug.Log("Tower Data:");
        foreach (var tower in towerDataAssets)
        {
            Debug.Log($"- {tower.name}: Cost={tower.cost}, Damage={tower.damage}, Range={tower.range}");
        }
        
        Debug.Log("Wave Data:");
        foreach (var wave in waveDataAssets)
        {
            Debug.Log($"- Wave {wave.waveNumber}: {wave.enemies.Count} enemy types");
        }
    }
}