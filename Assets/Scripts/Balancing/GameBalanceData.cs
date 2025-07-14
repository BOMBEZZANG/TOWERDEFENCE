using System;
using System.Collections.Generic;
using UnityEngine;

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
    
    // Wave timing data
    public List<WaveTimingData> wave_timings = new List<WaveTimingData>();
    public float total_wave_clear_time = 0f;
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
public class SessionDataWrapper
{
    public List<GameSessionData> sessions;
}

[Serializable]
public class WaveTimingData
{
    public int wave_number;
    public float wave_start_time;
    public float wave_clear_time;
    public int enemies_in_wave;
    public int enemies_killed;
    public int enemies_leaked;
}


[Serializable]
public class GameStats
{
    public float current_success_rate;
    public float current_avg_play_time;
    public int total_sessions;
    public int recent_sessions;
    public float average_wave_clear_time;
}