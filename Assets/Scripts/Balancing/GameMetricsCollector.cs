using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameMetricsCollector : MonoBehaviour
{
    public static GameMetricsCollector Instance;
    
    [Header("Data Collection Settings")]
    public bool enableDataCollection = true;
    public int maxSessionsToKeep = 50;
    
    private GameSessionData currentSession;
    private List<GameSessionData> allSessions = new List<GameSessionData>();
    private string saveFilePath;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDataCollection();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeDataCollection()
    {
        saveFilePath = Path.Combine(Application.dataPath, "..", "GameMetrics", "game_sessions_data.json");
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(saveFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        LoadSessionData();
        StartNewSession();
    }
    
    public void StartNewSession()
    {
        if (!enableDataCollection) return;
        
        currentSession = new GameSessionData
        {
            session_start = DateTime.Now,
            game_won = false,
            play_time_seconds = 0f,
            final_wave_reached = 0,
            money_spent = 0,
            towers_built = 0,
            enemies_killed = 0,
            lives_lost = 0,
            towers_used = new List<string>(),
            player_actions = new List<PlayerAction>(),
            wave_timings = new List<WaveTimingData>(),
            total_wave_clear_time = 0f
        };
        
        Debug.Log("[MetricsCollector] New game session started");
    }
    
    public void EndSession(bool gameWon, int finalWave)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.session_end = DateTime.Now;
        currentSession.game_won = gameWon;
        currentSession.play_time_seconds = (float)(currentSession.session_end - currentSession.session_start).TotalSeconds;
        currentSession.final_wave_reached = finalWave;
        
        allSessions.Add(currentSession);
        
        // Keep only the most recent sessions
        if (allSessions.Count > maxSessionsToKeep)
        {
            allSessions.RemoveAt(0);
        }
        
        SaveSessionData();
        Debug.Log($"[MetricsCollector] Session ended - Won: {gameWon}, Time: {currentSession.play_time_seconds:F1}s, Wave: {finalWave}");
    }
    
    public void RecordPlayerAction(string actionType, string details, Vector3 position = default)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        var action = new PlayerAction
        {
            timestamp = (float)(DateTime.Now - currentSession.session_start).TotalSeconds,
            action_type = actionType,
            action_details = details,
            position = position
        };
        
        currentSession.player_actions.Add(action);
    }
    
    public void RecordTowerBuilt(string towerType, int cost, Vector3 position)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.towers_built++;
        currentSession.money_spent += cost;
        
        if (!currentSession.towers_used.Contains(towerType))
        {
            currentSession.towers_used.Add(towerType);
        }
        
        RecordPlayerAction("build_tower", $"{towerType}:${cost}", position);
    }
    
    public void RecordTowerUpgraded(string towerType, int cost, Vector3 position)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.money_spent += cost;
        RecordPlayerAction("upgrade_tower", $"{towerType}:${cost}", position);
    }
    
    public void RecordTowerSold(string towerType, int sellValue, Vector3 position)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        RecordPlayerAction("sell_tower", $"{towerType}:${sellValue}", position);
    }
    
    public void RecordEnemyKilled()
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.enemies_killed++;
    }
    
    public void RecordLifeLost()
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.lives_lost++;
    }
    
    public void RecordWaveStarted(int waveNumber)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        currentSession.final_wave_reached = Mathf.Max(currentSession.final_wave_reached, waveNumber);
        RecordPlayerAction("start_wave", $"wave_{waveNumber}");
        
        // Start tracking this wave
        var waveData = new WaveTimingData
        {
            wave_number = waveNumber,
            wave_start_time = (float)(DateTime.Now - currentSession.session_start).TotalSeconds,
            wave_clear_time = 0f,
            enemies_in_wave = 0,
            enemies_killed = 0,
            enemies_leaked = 0
        };
        currentSession.wave_timings.Add(waveData);
    }
    
    public void RecordWaveCompleted(int waveNumber, int enemiesKilled, int enemiesLeaked)
    {
        if (!enableDataCollection || currentSession == null) return;
        
        var currentTime = (float)(DateTime.Now - currentSession.session_start).TotalSeconds;
        
        // Find the wave data and update it
        var waveData = currentSession.wave_timings.Find(w => w.wave_number == waveNumber);
        if (waveData != null)
        {
            waveData.wave_clear_time = currentTime - waveData.wave_start_time;
            waveData.enemies_killed = enemiesKilled;
            waveData.enemies_leaked = enemiesLeaked;
            waveData.enemies_in_wave = enemiesKilled + enemiesLeaked;
            
            currentSession.total_wave_clear_time += waveData.wave_clear_time;
        }
    }
    
    
    
    
    
    public List<GameSessionData> GetRecentSessions(int count = 10)
    {
        int startIndex = Mathf.Max(0, allSessions.Count - count);
        return allSessions.GetRange(startIndex, allSessions.Count - startIndex);
    }
    
    public GameStats GetCurrentStats()
    {
        if (allSessions.Count == 0) return new GameStats();
        
        var recent = GetRecentSessions(20);
        float successRate = (float)recent.FindAll(s => s.game_won).Count / recent.Count;
        float avgPlayTime = 0f;
        
        foreach (var session in recent)
        {
            avgPlayTime += session.play_time_seconds;
        }
        avgPlayTime /= recent.Count;
        
        return new GameStats
        {
            current_success_rate = successRate,
            current_avg_play_time = avgPlayTime,
            total_sessions = allSessions.Count,
            recent_sessions = recent.Count
        };
    }
    
    private void SaveSessionData()
    {
        try
        {
            string json = JsonUtility.ToJson(new SessionDataWrapper { sessions = allSessions }, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[MetricsCollector] Failed to save session data: {e.Message}");
        }
    }
    
    private void LoadSessionData()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                var wrapper = JsonUtility.FromJson<SessionDataWrapper>(json);
                allSessions = wrapper.sessions ?? new List<GameSessionData>();
                Debug.Log($"[MetricsCollector] Loaded {allSessions.Count} previous sessions");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[MetricsCollector] Failed to load session data: {e.Message}");
            allSessions = new List<GameSessionData>();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveSessionData();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveSessionData();
    }
}

