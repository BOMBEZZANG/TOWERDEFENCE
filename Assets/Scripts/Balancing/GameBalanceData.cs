using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameBalanceData
{
    [Header("Target Metrics")]
    public float success_rate_target = 0.9f;
    public float play_time_target = 180f; // 3 minutes

    [Header("Current Game State")]
    public int starting_money = 100;
    public int starting_lives = 20;
    
    [Header("Enemy Balance")]
    public List<EnemyBalanceData> enemies = new List<EnemyBalanceData>();
    
    [Header("Tower Balance")]
    public List<TowerBalanceData> towers = new List<TowerBalanceData>();
    
    [Header("Wave Balance")]
    public List<WaveBalanceData> waves = new List<WaveBalanceData>();
    
    [Header("Node Balance")]
    public int available_node_count = 12;
    public float node_spacing = 2.0f;
}

[Serializable]
public class EnemyBalanceData
{
    public string enemy_name;
    public float health = 100f;
    public float speed = 5f;
    public int reward = 10;
    public EnemyType enemy_type;
}

[Serializable]
public class TowerBalanceData
{
    public string tower_name;
    public int cost = 50;
    public float damage = 25f;
    public float range = 3f;
    public float fire_rate = 1f;
    public int upgrade_cost = 75;
    public TowerType tower_type;
}

[Serializable]
public class WaveBalanceData
{
    public int wave_number;
    public List<EnemySpawnData> enemy_spawns = new List<EnemySpawnData>();
    public float spawn_delay = 0.5f;
    public float time_between_waves = 5f;
}

[Serializable]
public class EnemySpawnData
{
    public string enemy_type;
    public int count;
    public float spawn_interval = 0.5f;
}

[Serializable]
public class GameSessionData
{
    public DateTime session_start;
    public DateTime session_end;
    public bool game_won;
    public float play_time_seconds;
    public int final_wave_reached;
    public int money_spent;
    public int towers_built;
    public int enemies_killed;
    public int lives_lost;
    public List<string> towers_used = new List<string>();
    public List<PlayerAction> player_actions = new List<PlayerAction>();
}

[Serializable]
public class PlayerAction
{
    public float timestamp;
    public string action_type; // "build_tower", "upgrade_tower", "sell_tower", "start_wave"
    public string action_details;
    public Vector3 position;
}

[Serializable]
public class BalanceAnalysisRequest
{
    public GameBalanceData current_balance;
    public List<GameSessionData> recent_sessions;
    public BalanceTargets targets;
}

[Serializable]
public class BalanceTargets
{
    public float success_rate_target = 0.9f;
    public float play_time_target = 180f;
    public float difficulty_curve_target = 1.2f; // How much harder each wave should be
}

[Serializable]
public class BalanceAdjustmentResponse
{
    public string analysis_summary;
    public List<BalanceAdjustment> suggested_adjustments;
    public float predicted_success_rate;
    public float predicted_play_time;
    public string reasoning;
}

[Serializable]
public class BalanceAdjustment
{
    public string category; // "enemy", "tower", "wave", "economy", "nodes"
    public string target_name;
    public string property_name;
    public float current_value;
    public float suggested_value;
    public string reason;
    public float confidence; // 0-1 how confident Claude is in this adjustment
}