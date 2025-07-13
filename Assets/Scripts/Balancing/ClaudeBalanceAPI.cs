using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ClaudeBalanceAPI : MonoBehaviour
{
    public static ClaudeBalanceAPI Instance;
    
    [Header("API Configuration")]
    [SerializeField] private string claudeApiKey = ""; // Set this in inspector or via code
    [SerializeField] private string apiUrl = "https://api.anthropic.com/v1/messages";
    [SerializeField] private bool enableAPI = true;
    
    [Header("Balance Settings")]
    [SerializeField] private float analysisInterval = 300f; // 5 minutes
    [SerializeField] private int minSessionsForAnalysis = 5;
    
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
    
    private void Start()
    {
        if (enableAPI && !string.IsNullOrEmpty(claudeApiKey))
        {
            InvokeRepeating(nameof(PerformPeriodicAnalysis), analysisInterval, analysisInterval);
        }
    }
    
    public void SetApiKey(string apiKey)
    {
        claudeApiKey = apiKey;
        enableAPI = !string.IsNullOrEmpty(apiKey);
    }
    
    private void PerformPeriodicAnalysis()
    {
        if (GameMetricsCollector.Instance == null) return;
        
        var sessions = GameMetricsCollector.Instance.GetRecentSessions(20);
        if (sessions.Count >= minSessionsForAnalysis)
        {
            StartCoroutine(RequestBalanceAnalysis());
        }
    }
    
    public void RequestBalanceAnalysisNow()
    {
        if (enableAPI)
        {
            StartCoroutine(RequestBalanceAnalysis());
        }
        else
        {
            Debug.LogWarning("[ClaudeBalanceAPI] API is disabled or API key not set");
        }
    }
    
    private IEnumerator RequestBalanceAnalysis()
    {
        Debug.Log("[ClaudeBalanceAPI] Requesting balance analysis from Claude...");
        
        // Gather current game data
        var currentBalance = GameBalanceManager.Instance.GetCurrentBalanceData();
        var recentSessions = GameMetricsCollector.Instance.GetRecentSessions(15);
        var currentStats = GameMetricsCollector.Instance.GetCurrentStats();
        
        // Create analysis request
        var analysisPrompt = CreateAnalysisPrompt(currentBalance, recentSessions, currentStats);
        
        // Send request to Claude API
        yield return StartCoroutine(SendClaudeRequest(analysisPrompt, OnBalanceAnalysisReceived));
    }
    
    private string CreateAnalysisPrompt(GameBalanceData currentBalance, List<GameSessionData> sessions, GameStats stats)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("You are a game balance expert analyzing a tower defense game. Your goal is to suggest balance changes to achieve these targets:");
        prompt.AppendLine($"- Success Rate Target: {currentBalance.success_rate_target * 100:F1}%");
        prompt.AppendLine($"- Play Time Target: {currentBalance.play_time_target} seconds");
        prompt.AppendLine();
        
        prompt.AppendLine("Current Performance:");
        prompt.AppendLine($"- Current Success Rate: {stats.current_success_rate * 100:F1}%");
        prompt.AppendLine($"- Current Average Play Time: {stats.current_avg_play_time:F1} seconds");
        prompt.AppendLine($"- Sessions Analyzed: {stats.recent_sessions}");
        prompt.AppendLine();
        
        prompt.AppendLine("Current Game Balance:");
        prompt.AppendLine($"- Starting Money: ${currentBalance.starting_money}");
        prompt.AppendLine($"- Starting Lives: {currentBalance.starting_lives}");
        prompt.AppendLine($"- Available Nodes: {currentBalance.available_node_count}");
        prompt.AppendLine();
        
        prompt.AppendLine("Enemy Data:");
        foreach (var enemy in currentBalance.enemies)
        {
            prompt.AppendLine($"- {enemy.enemy_name}: Health={enemy.health}, Speed={enemy.speed}, Reward=${enemy.reward}");
        }
        prompt.AppendLine();
        
        prompt.AppendLine("Tower Data:");
        foreach (var tower in currentBalance.towers)
        {
            prompt.AppendLine($"- {tower.tower_name}: Cost=${tower.cost}, Damage={tower.damage}, Range={tower.range}, FireRate={tower.fire_rate}, UpgradeCost=${tower.upgrade_cost}");
        }
        prompt.AppendLine();
        
        prompt.AppendLine("Wave Data:");
        for (int i = 0; i < Mathf.Min(5, currentBalance.waves.Count); i++)
        {
            var wave = currentBalance.waves[i];
            prompt.AppendLine($"- Wave {wave.wave_number}: {wave.enemy_spawns.Count} enemy types, Delay={wave.spawn_delay}s");
        }
        prompt.AppendLine();
        
        // Add session analysis
        if (sessions.Count > 0)
        {
            prompt.AppendLine("Recent Session Analysis:");
            int wins = 0;
            float totalTime = 0;
            foreach (var session in sessions)
            {
                if (session.game_won) wins++;
                totalTime += session.play_time_seconds;
                prompt.AppendLine($"- Session: Won={session.game_won}, Time={session.play_time_seconds:F1}s, Wave={session.final_wave_reached}, Towers={session.towers_built}");
            }
            prompt.AppendLine();
        }
        
        prompt.AppendLine("Please provide balance suggestions in this JSON format:");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"analysis_summary\": \"Brief analysis of current state\",");
        prompt.AppendLine("  \"suggested_adjustments\": [");
        prompt.AppendLine("    {");
        prompt.AppendLine("      \"category\": \"enemy|tower|wave|economy|nodes\",");
        prompt.AppendLine("      \"target_name\": \"specific item name\",");
        prompt.AppendLine("      \"property_name\": \"health|damage|cost|etc\",");
        prompt.AppendLine("      \"current_value\": 100,");
        prompt.AppendLine("      \"suggested_value\": 120,");
        prompt.AppendLine("      \"reason\": \"explanation\",");
        prompt.AppendLine("      \"confidence\": 0.8");
        prompt.AppendLine("    }");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"predicted_success_rate\": 0.9,");
        prompt.AppendLine("  \"predicted_play_time\": 180,");
        prompt.AppendLine("  \"reasoning\": \"Detailed explanation of suggestions\"");
        prompt.AppendLine("}");
        
        return prompt.ToString();
    }
    
    private IEnumerator SendClaudeRequest(string prompt, System.Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(claudeApiKey))
        {
            Debug.LogError("[ClaudeBalanceAPI] API key not set!");
            yield break;
        }
        
        var requestData = new
        {
            model = "claude-3-sonnet-20240229",
            max_tokens = 2000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", claudeApiKey);
            request.SetRequestHeader("anthropic-version", "2023-06-01");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<ClaudeResponse>(request.downloadHandler.text);
                    if (response.content != null && response.content.Length > 0)
                    {
                        onComplete?.Invoke(response.content[0].text);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ClaudeBalanceAPI] Failed to parse response: {e.Message}");
                    Debug.LogError($"Raw response: {request.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"[ClaudeBalanceAPI] Request failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }
    
    private void OnBalanceAnalysisReceived(string claudeResponse)
    {
        Debug.Log($"[ClaudeBalanceAPI] Received analysis from Claude");
        
        try
        {
            // Extract JSON from Claude's response (it might include extra text)
            string jsonContent = ExtractJsonFromResponse(claudeResponse);
            
            if (!string.IsNullOrEmpty(jsonContent))
            {
                var analysis = JsonUtility.FromJson<BalanceAdjustmentResponse>(jsonContent);
                GameBalanceManager.Instance?.ApplyBalanceAnalysis(analysis);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ClaudeBalanceAPI] Failed to process Claude response: {e.Message}");
            Debug.LogError($"Claude response was: {claudeResponse}");
        }
    }
    
    private string ExtractJsonFromResponse(string response)
    {
        // Find JSON content between curly braces
        int startIndex = response.IndexOf('{');
        int endIndex = response.LastIndexOf('}');
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return response.Substring(startIndex, endIndex - startIndex + 1);
        }
        
        return "";
    }
    
    [Serializable]
    private class ClaudeResponse
    {
        public ClaudeContent[] content;
    }
    
    [Serializable]
    private class ClaudeContent
    {
        public string text;
        public string type;
    }
}