using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClaudeBalanceAPI : MonoBehaviour
{
    public static ClaudeBalanceAPI Instance;
    
    [Header("UI Settings")]
    public bool showWindow = false;
    public Rect windowRect = new Rect(20, 20, 600, 400);
    
    [Header("API Settings")]
    private string apiKey = "";
    private string customPrompt = "You are a game balance expert analyzing a tower defense game. I'm providing you with two data files:\n\n1. BALANCE_DATA.json - Contains current game configuration (towers, enemies, waves, targets)\n2. PLAYER SESSION DATA - Contains actual player gameplay sessions\n\nPlease analyze both files and provide:\n- Player performance analysis\n- Balance issues identification\n- Specific suggestions for improvement\n- Predicted impact of suggested changes\n\nFocus on data-driven insights and actionable recommendations.";
    private string apiUrl = "https://api.anthropic.com/v1/messages";
    
    private bool isProcessing = false;
    private string statusMessage = "";
    
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
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showWindow = !showWindow;
            Debug.Log($"[ClaudeBalanceAPI] F1 pressed! Window is now: {(showWindow ? "OPEN" : "CLOSED")}");
        }
    }
    
    private void Start()
    {
        Debug.Log("[ClaudeBalanceAPI] ClaudeBalanceAPI started and ready! Press F1 to open analysis window.");
    }
    
    private void OnGUI()
    {
        if (showWindow)
        {
            windowRect = GUI.Window(0, windowRect, DrawWindow, "Claude API Analysis Tool");
        }
    }
    
    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // Session Info
        GUILayout.Label("Session Information:", CustomStyles.boldLabel);
        if (GameMetricsCollector.Instance != null)
        {
            var stats = GameMetricsCollector.Instance.GetCurrentStats();
            GUILayout.Label($"Total Sessions: {stats.total_sessions}");
            GUILayout.Label($"Recent Sessions: {stats.recent_sessions}");
            GUILayout.Label($"Success Rate: {stats.current_success_rate * 100:F1}%");
            GUILayout.Label($"Avg Play Time: {stats.current_avg_play_time:F1}s");
        }
        else
        {
            GUILayout.Label("GameMetricsCollector not found!");
        }
        
        GUILayout.Space(10);
        
        // API Key Input
        GUILayout.Label("API Key:", CustomStyles.boldLabel);
        apiKey = GUILayout.TextField(apiKey, GUILayout.Height(20));
        
        GUILayout.Space(10);
        
        // Custom Prompt Input
        GUILayout.Label("Custom Prompt:", CustomStyles.boldLabel);
        customPrompt = GUILayout.TextArea(customPrompt, GUILayout.Height(100));
        
        GUILayout.Space(10);
        
        // Send Button
        GUI.enabled = !isProcessing && !string.IsNullOrEmpty(apiKey);
        if (GUILayout.Button(isProcessing ? "Processing..." : "Send to Claude API", GUILayout.Height(30)))
        {
            StartCoroutine(SendToClaudeAPI());
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        // Status Message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUILayout.Label("Status:", CustomStyles.boldLabel);
            GUILayout.Label(statusMessage);
        }
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
    
    private IEnumerator SendToClaudeAPI()
    {
        isProcessing = true;
        statusMessage = "Reading game data files...";
        
        // Read the session data JSON file
        string sessionDataPath = Path.Combine(Application.dataPath, "..", "GameMetrics", "game_sessions_data.json");
        string balanceDataPath = Path.Combine(Application.dataPath, "..", "GameMetrics", "BALANCE_DATA.json");
        
        if (!File.Exists(sessionDataPath))
        {
            statusMessage = "Error: game_sessions_data.json not found!";
            isProcessing = false;
            yield break;
        }
        
        if (!File.Exists(balanceDataPath))
        {
            statusMessage = "Error: BALANCE_DATA.json not found!";
            isProcessing = false;
            yield break;
        }
        
        string sessionDataContent = File.ReadAllText(sessionDataPath);
        string balanceDataContent = File.ReadAllText(balanceDataPath);
        
        // Create the full prompt with both files
        string fullPrompt = customPrompt + 
            "\n\n=== GAME BALANCE DATA ===\n" + balanceDataContent + 
            "\n\n=== PLAYER SESSION DATA ===\n" + sessionDataContent;
        
        statusMessage = "Sending request to Claude API...";
        
        // Create request data
        var requestData = new ClaudeRequest
        {
            model = "claude-3-7-sonnet-20250219",
            max_tokens = 4000,
            messages = new ClaudeMessage[]
            {
                new ClaudeMessage { role = "user", content = fullPrompt }
            }
        };
        
        string requestJson = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);
            request.SetRequestHeader("anthropic-version", "2023-06-01");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Parse Claude's response
                    var response = JsonUtility.FromJson<ClaudeResponse>(request.downloadHandler.text);
                    if (response.content != null && response.content.Length > 0)
                    {
                        string claudeAnalysis = response.content[0].text;
                        
                        // Save response to file
                        string outputPath = Path.Combine(Application.dataPath, "..", "GameMetrics", 
                            $"claude_analysis_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
                        
                        File.WriteAllText(outputPath, claudeAnalysis);
                        
                        statusMessage = $"Success! Analysis saved to: {Path.GetFileName(outputPath)}";
                    }
                    else
                    {
                        statusMessage = "Error: Empty response from Claude API";
                    }
                }
                catch (Exception e)
                {
                    statusMessage = $"Error parsing response: {e.Message}";
                    Debug.LogError($"Claude API Response: {request.downloadHandler.text}");
                }
            }
            else
            {
                statusMessage = $"API Error: {request.error}";
                Debug.LogError($"Claude API Error: {request.downloadHandler.text}");
            }
        }
        
        isProcessing = false;
    }
    
    [Serializable]
    private class ClaudeRequest
    {
        public string model;
        public int max_tokens;
        public ClaudeMessage[] messages;
    }
    
    [Serializable]
    private class ClaudeMessage
    {
        public string role;
        public string content;
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

public static class CustomStyles
{
    public static GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
}