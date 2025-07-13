using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalanceSystemUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject balancePanel;
    public Button openBalanceButton;
    public Button closeBalanceButton;
    public Button analyzeNowButton;
    public Button applyAdjustmentsButton;
    public Button resetBalanceButton;
    
    [Header("Stats Display")]
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI sessionsCountText;
    public TextMeshProUGUI lastAnalysisText;
    
    [Header("API Settings")]
    public TMP_InputField apiKeyInput;
    public Button setApiKeyButton;
    public Toggle autoApplyToggle;
    
    private void Start()
    {
        SetupUI();
        UpdateStatsDisplay();
        InvokeRepeating(nameof(UpdateStatsDisplay), 1f, 5f); // Update every 5 seconds
    }
    
    private void SetupUI()
    {
        if (balancePanel != null) balancePanel.SetActive(false);
        
        if (openBalanceButton != null) openBalanceButton.onClick.AddListener(OpenBalancePanel);
        if (closeBalanceButton != null) closeBalanceButton.onClick.AddListener(CloseBalancePanel);
        if (analyzeNowButton != null) analyzeNowButton.onClick.AddListener(RequestAnalysis);
        if (applyAdjustmentsButton != null) applyAdjustmentsButton.onClick.AddListener(ApplyPendingAdjustments);
        if (resetBalanceButton != null) resetBalanceButton.onClick.AddListener(ResetBalance);
        if (setApiKeyButton != null) setApiKeyButton.onClick.AddListener(SetApiKey);
        
        if (autoApplyToggle != null)
        {
            autoApplyToggle.isOn = GameBalanceManager.Instance?.autoApplyAdjustments ?? true;
            autoApplyToggle.onValueChanged.AddListener(OnAutoApplyToggled);
        }
    }
    
    private void OpenBalancePanel()
    {
        if (balancePanel != null) balancePanel.SetActive(true);
        UpdateStatsDisplay();
    }
    
    private void CloseBalancePanel()
    {
        if (balancePanel != null) balancePanel.SetActive(false);
    }
    
    private void UpdateStatsDisplay()
    {
        if (GameMetricsCollector.Instance == null) return;
        
        var stats = GameMetricsCollector.Instance.GetCurrentStats();
        
        if (successRateText != null)
        {
            successRateText.text = $"Success Rate: {stats.current_success_rate * 100:F1}% (Target: 90%)";
            successRateText.color = GetStatusColor(stats.current_success_rate, 0.9f);
        }
        
        if (playTimeText != null)
        {
            playTimeText.text = $"Avg Play Time: {stats.current_avg_play_time:F1}s (Target: 180s)";
            playTimeText.color = GetStatusColor(1f - Mathf.Abs(stats.current_avg_play_time - 180f) / 180f, 0.8f);
        }
        
        if (sessionsCountText != null)
        {
            sessionsCountText.text = $"Sessions: {stats.recent_sessions} recent, {stats.total_sessions} total";
        }
        
        if (lastAnalysisText != null && GameBalanceManager.Instance != null)
        {
            var lastAnalysis = GameBalanceManager.Instance.GetLastAnalysis();
            if (lastAnalysis != null)
            {
                lastAnalysisText.text = $"Last Analysis: {lastAnalysis.suggested_adjustments?.Count ?? 0} suggestions\n" +
                                      $"Predicted: {lastAnalysis.predicted_success_rate * 100:F1}% success, {lastAnalysis.predicted_play_time:F1}s";
            }
            else
            {
                lastAnalysisText.text = "No analysis yet";
            }
        }
        
        // Update button states
        if (applyAdjustmentsButton != null && GameBalanceManager.Instance != null)
        {
            var lastAnalysis = GameBalanceManager.Instance.GetLastAnalysis();
            applyAdjustmentsButton.interactable = lastAnalysis?.suggested_adjustments?.Count > 0;
        }
    }
    
    private Color GetStatusColor(float currentValue, float targetValue)
    {
        float difference = Mathf.Abs(currentValue - targetValue);
        if (difference < 0.1f) return Color.green;
        if (difference < 0.2f) return Color.yellow;
        return Color.red;
    }
    
    private void RequestAnalysis()
    {
        if (ClaudeBalanceAPI.Instance != null)
        {
            ClaudeBalanceAPI.Instance.RequestBalanceAnalysisNow();
            
            if (lastAnalysisText != null)
            {
                lastAnalysisText.text = "Requesting analysis from Claude...";
            }
        }
        else
        {
            Debug.LogWarning("ClaudeBalanceAPI instance not found!");
        }
    }
    
    private void ApplyPendingAdjustments()
    {
        if (GameBalanceManager.Instance != null)
        {
            GameBalanceManager.Instance.ApplyPendingAdjustments();
            
            if (lastAnalysisText != null)
            {
                lastAnalysisText.text = "Applied pending adjustments";
            }
        }
    }
    
    private void ResetBalance()
    {
        if (GameBalanceManager.Instance != null)
        {
            GameBalanceManager.Instance.ResetToDefaults();
            
            if (lastAnalysisText != null)
            {
                lastAnalysisText.text = "Reset balance to defaults";
            }
        }
    }
    
    private void SetApiKey()
    {
        if (apiKeyInput != null && ClaudeBalanceAPI.Instance != null)
        {
            string apiKey = apiKeyInput.text.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                ClaudeBalanceAPI.Instance.SetApiKey(apiKey);
                apiKeyInput.text = ""; // Clear for security
                
                if (lastAnalysisText != null)
                {
                    lastAnalysisText.text = "API key set successfully";
                }
            }
        }
    }
    
    private void OnAutoApplyToggled(bool value)
    {
        if (GameBalanceManager.Instance != null)
        {
            GameBalanceManager.Instance.autoApplyAdjustments = value;
        }
    }
    
    // Keyboard shortcut to open balance panel
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftControl))
        {
            if (balancePanel != null)
            {
                balancePanel.SetActive(!balancePanel.activeInHierarchy);
                if (balancePanel.activeInHierarchy)
                {
                    UpdateStatsDisplay();
                }
            }
        }
    }
}