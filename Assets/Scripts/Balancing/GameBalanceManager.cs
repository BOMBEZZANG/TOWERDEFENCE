using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class GameBalanceManager : MonoBehaviour
{
    public static GameBalanceManager Instance;
    
    [Header("Balance Configuration")]
    public GameBalanceData currentBalance;
    public bool autoApplyAdjustments = true;
    public float confidenceThreshold = 0.7f;
    
    [Header("Data Sources")]
    public List<EnemyData> enemyDataAssets;
    public List<TowerData> towerDataAssets;
    public List<WaveData> waveDataAssets;
    
    private string balanceConfigPath;
    private BalanceAdjustmentResponse lastAnalysis;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBalanceManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeBalanceManager()
    {
        balanceConfigPath = Path.Combine(Application.persistentDataPath, "balance_config.json");
        
        if (currentBalance == null)
        {
            currentBalance = CreateDefaultBalanceData();
        }
        
        LoadBalanceConfiguration();
        SyncCurrentDataWithAssets();
    }
    
    private GameBalanceData CreateDefaultBalanceData()
    {
        var balance = new GameBalanceData();
        
        // Set default targets
        balance.success_rate_target = 0.9f;
        balance.play_time_target = 180f;
        
        // Set default game state
        balance.starting_money = 100;
        balance.starting_lives = 20;
        balance.available_node_count = 12;
        
        return balance;
    }
    
    public void SyncCurrentDataWithAssets()
    {
        // Sync enemy data
        currentBalance.enemies.Clear();
        foreach (var enemyAsset in enemyDataAssets)
        {
            if (enemyAsset != null)
            {
                currentBalance.enemies.Add(new EnemyBalanceData
                {
                    enemy_name = enemyAsset.enemyName,
                    health = enemyAsset.health,
                    speed = enemyAsset.speed,
                    reward = enemyAsset.reward,
                    enemy_type = enemyAsset.enemyType
                });
            }
        }
        
        // Sync tower data
        currentBalance.towers.Clear();
        foreach (var towerAsset in towerDataAssets)
        {
            if (towerAsset != null)
            {
                currentBalance.towers.Add(new TowerBalanceData
                {
                    tower_name = towerAsset.towerName,
                    cost = towerAsset.cost,
                    damage = towerAsset.damage,
                    range = towerAsset.range,
                    fire_rate = towerAsset.fireRate,
                    upgrade_cost = towerAsset.upgradeCost,
                    tower_type = towerAsset.towerType
                });
            }
        }
        
        // Sync wave data
        currentBalance.waves.Clear();
        foreach (var waveAsset in waveDataAssets)
        {
            if (waveAsset != null)
            {
                var waveBalance = new WaveBalanceData
                {
                    wave_number = waveAsset.waveNumber,
                    spawn_delay = waveAsset.spawnDelay,
                    time_between_waves = waveAsset.timeBetweenWaves
                };
                
                foreach (var enemySpawn in waveAsset.enemies)
                {
                    waveBalance.enemy_spawns.Add(new EnemySpawnData
                    {
                        enemy_type = enemySpawn.enemyData?.enemyName ?? "Unknown",
                        count = enemySpawn.count,
                        spawn_interval = enemySpawn.spawnDelay
                    });
                }
                
                currentBalance.waves.Add(waveBalance);
            }
        }
        
        // Sync game manager settings
        if (GameManager.Instance != null)
        {
            currentBalance.starting_money = GameManager.Instance.startingMoney;
            currentBalance.starting_lives = GameManager.Instance.startingLives;
        }
        
        SaveBalanceConfiguration();
    }
    
    public GameBalanceData GetCurrentBalanceData()
    {
        return currentBalance;
    }
    
    public void ApplyBalanceAnalysis(BalanceAdjustmentResponse analysis)
    {
        if (analysis == null)
        {
            Debug.LogError("[GameBalanceManager] Received null analysis");
            return;
        }
        
        lastAnalysis = analysis;
        
        Debug.Log($"[GameBalanceManager] Received balance analysis:");
        Debug.Log($"Summary: {analysis.analysis_summary}");
        Debug.Log($"Predicted Success Rate: {analysis.predicted_success_rate * 100:F1}%");
        Debug.Log($"Predicted Play Time: {analysis.predicted_play_time:F1}s");
        Debug.Log($"Suggestions: {analysis.suggested_adjustments?.Count ?? 0}");
        
        if (autoApplyAdjustments && analysis.suggested_adjustments != null)
        {
            ApplyAdjustments(analysis.suggested_adjustments);
        }
        else
        {
            Debug.Log("[GameBalanceManager] Auto-apply disabled. Use ApplyPendingAdjustments() to apply manually.");
        }
    }
    
    public void ApplyPendingAdjustments()
    {
        if (lastAnalysis?.suggested_adjustments != null)
        {
            ApplyAdjustments(lastAnalysis.suggested_adjustments);
        }
    }
    
    private void ApplyAdjustments(List<BalanceAdjustment> adjustments)
    {
        int appliedCount = 0;
        
        foreach (var adjustment in adjustments)
        {
            if (adjustment.confidence >= confidenceThreshold)
            {
                if (ApplySingleAdjustment(adjustment))
                {
                    appliedCount++;
                    Debug.Log($"[GameBalanceManager] Applied: {adjustment.category}.{adjustment.target_name}.{adjustment.property_name} = {adjustment.suggested_value} (was {adjustment.current_value})");
                }
            }
            else
            {
                Debug.Log($"[GameBalanceManager] Skipped low confidence adjustment: {adjustment.target_name}.{adjustment.property_name} (confidence: {adjustment.confidence})");
            }
        }
        
        if (appliedCount > 0)
        {
            SaveBalanceConfiguration();
            ApplyChangesToGameAssets();
            Debug.Log($"[GameBalanceManager] Applied {appliedCount} balance adjustments");
        }
    }
    
    private bool ApplySingleAdjustment(BalanceAdjustment adjustment)
    {
        try
        {
            switch (adjustment.category.ToLower())
            {
                case "enemy":
                    return ApplyEnemyAdjustment(adjustment);
                case "tower":
                    return ApplyTowerAdjustment(adjustment);
                case "wave":
                    return ApplyWaveAdjustment(adjustment);
                case "economy":
                    return ApplyEconomyAdjustment(adjustment);
                case "nodes":
                    return ApplyNodeAdjustment(adjustment);
                default:
                    Debug.LogWarning($"[GameBalanceManager] Unknown adjustment category: {adjustment.category}");
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBalanceManager] Failed to apply adjustment {adjustment.target_name}.{adjustment.property_name}: {e.Message}");
            return false;
        }
    }
    
    private bool ApplyEnemyAdjustment(BalanceAdjustment adjustment)
    {
        var enemy = currentBalance.enemies.FirstOrDefault(e => e.enemy_name == adjustment.target_name);
        if (enemy == null) return false;
        
        switch (adjustment.property_name.ToLower())
        {
            case "health": enemy.health = adjustment.suggested_value; return true;
            case "speed": enemy.speed = adjustment.suggested_value; return true;
            case "reward": enemy.reward = (int)adjustment.suggested_value; return true;
            default: return false;
        }
    }
    
    private bool ApplyTowerAdjustment(BalanceAdjustment adjustment)
    {
        var tower = currentBalance.towers.FirstOrDefault(t => t.tower_name == adjustment.target_name);
        if (tower == null) return false;
        
        switch (adjustment.property_name.ToLower())
        {
            case "cost": tower.cost = (int)adjustment.suggested_value; return true;
            case "damage": tower.damage = adjustment.suggested_value; return true;
            case "range": tower.range = adjustment.suggested_value; return true;
            case "fire_rate": case "firerate": tower.fire_rate = adjustment.suggested_value; return true;
            case "upgrade_cost": case "upgradecost": tower.upgrade_cost = (int)adjustment.suggested_value; return true;
            default: return false;
        }
    }
    
    private bool ApplyWaveAdjustment(BalanceAdjustment adjustment)
    {
        var wave = currentBalance.waves.FirstOrDefault(w => w.wave_number.ToString() == adjustment.target_name);
        if (wave == null) return false;
        
        switch (adjustment.property_name.ToLower())
        {
            case "spawn_delay": case "spawndelay": wave.spawn_delay = adjustment.suggested_value; return true;
            case "time_between_waves": case "timebetweenwaves": wave.time_between_waves = adjustment.suggested_value; return true;
            default: return false;
        }
    }
    
    private bool ApplyEconomyAdjustment(BalanceAdjustment adjustment)
    {
        switch (adjustment.property_name.ToLower())
        {
            case "starting_money": case "startingmoney":
                currentBalance.starting_money = (int)adjustment.suggested_value;
                return true;
            case "starting_lives": case "startinglives":
                currentBalance.starting_lives = (int)adjustment.suggested_value;
                return true;
            default: return false;
        }
    }
    
    private bool ApplyNodeAdjustment(BalanceAdjustment adjustment)
    {
        switch (adjustment.property_name.ToLower())
        {
            case "available_node_count": case "availablenodecount":
                currentBalance.available_node_count = (int)adjustment.suggested_value;
                return true;
            case "node_spacing": case "nodespacing":
                currentBalance.node_spacing = adjustment.suggested_value;
                return true;
            default: return false;
        }
    }
    
    private void ApplyChangesToGameAssets()
    {
        // Apply enemy changes
        for (int i = 0; i < Mathf.Min(enemyDataAssets.Count, currentBalance.enemies.Count); i++)
        {
            if (enemyDataAssets[i] != null)
            {
                var balanceData = currentBalance.enemies[i];
                enemyDataAssets[i].health = balanceData.health;
                enemyDataAssets[i].speed = balanceData.speed;
                enemyDataAssets[i].reward = balanceData.reward;
            }
        }
        
        // Apply tower changes
        for (int i = 0; i < Mathf.Min(towerDataAssets.Count, currentBalance.towers.Count); i++)
        {
            if (towerDataAssets[i] != null)
            {
                var balanceData = currentBalance.towers[i];
                towerDataAssets[i].cost = balanceData.cost;
                towerDataAssets[i].damage = balanceData.damage;
                towerDataAssets[i].range = balanceData.range;
                towerDataAssets[i].fireRate = balanceData.fire_rate;
                towerDataAssets[i].upgradeCost = balanceData.upgrade_cost;
            }
        }
        
        // Apply game manager changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.startingMoney = currentBalance.starting_money;
            GameManager.Instance.startingLives = currentBalance.starting_lives;
            GameManager.Instance.currentMoney = currentBalance.starting_money;
            GameManager.Instance.currentLives = currentBalance.starting_lives;
        }
    }
    
    public void SaveBalanceConfiguration()
    {
        try
        {
            string json = JsonUtility.ToJson(currentBalance, true);
            File.WriteAllText(balanceConfigPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBalanceManager] Failed to save balance config: {e.Message}");
        }
    }
    
    public void LoadBalanceConfiguration()
    {
        try
        {
            if (File.Exists(balanceConfigPath))
            {
                string json = File.ReadAllText(balanceConfigPath);
                var loadedBalance = JsonUtility.FromJson<GameBalanceData>(json);
                if (loadedBalance != null)
                {
                    currentBalance = loadedBalance;
                    Debug.Log("[GameBalanceManager] Loaded balance configuration");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBalanceManager] Failed to load balance config: {e.Message}");
        }
    }
    
    public BalanceAdjustmentResponse GetLastAnalysis()
    {
        return lastAnalysis;
    }
    
    public void ResetToDefaults()
    {
        currentBalance = CreateDefaultBalanceData();
        SyncCurrentDataWithAssets();
        SaveBalanceConfiguration();
        Debug.Log("[GameBalanceManager] Reset balance to defaults");
    }
}