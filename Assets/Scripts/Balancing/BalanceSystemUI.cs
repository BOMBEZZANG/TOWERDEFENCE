using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalanceSystemUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject balancePanel;
    public Button openBalanceButton;
    public Button closeBalanceButton;
    public Button openF1WindowButton;
    
    [Header("Stats Display")]
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI sessionsCountText;
    public TextMeshProUGUI lastAnalysisText;
    
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
        if (openF1WindowButton != null) openF1WindowButton.onClick.AddListener(OpenF1Window);
    }
    
    private void UpdateStatsDisplay()
    {
        if (GameMetricsCollector.Instance == null) return;
        
        var stats = GameMetricsCollector.Instance.GetCurrentStats();
        
        if (successRateText != null)
            successRateText.text = $"Success Rate: {stats.current_success_rate * 100:F1}%";
        
        if (playTimeText != null)
            playTimeText.text = $"Avg Play Time: {stats.current_avg_play_time:F1}s";
        
        if (sessionsCountText != null)
            sessionsCountText.text = $"Total Sessions: {stats.total_sessions}";
        
        if (lastAnalysisText != null)
            lastAnalysisText.text = "Press F1 to open Claude API Analysis Tool";
    }
    
    private void OpenBalancePanel()
    {
        if (balancePanel != null) balancePanel.SetActive(true);
    }
    
    private void CloseBalancePanel()
    {
        if (balancePanel != null) balancePanel.SetActive(false);
    }
    
    private void OpenF1Window()
    {
        if (ClaudeBalanceAPI.Instance != null)
        {
            ClaudeBalanceAPI.Instance.showWindow = true;
        }
    }
    
    private void Update()
    {
        // Show instructions
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("F1 pressed - Claude API Analysis window toggled");
        }
    }
}