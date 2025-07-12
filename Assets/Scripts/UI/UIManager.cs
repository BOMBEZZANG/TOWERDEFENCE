using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Game UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;
    public Button startWaveButton;
    
    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public GameObject winUI;
    public Button restartButton;
    public Button winRestartButton;
    
    [Header("Node UI")]
    public GameObject nodeUI;
    public Button sellButton;
    public Button upgradeButton;
    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        gameOverUI.SetActive(false);
        winUI.SetActive(false);
        nodeUI.SetActive(false);
        
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartWave);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(Restart);
        }
        
        if (winRestartButton != null)
        {
            winRestartButton.onClick.AddListener(Restart);
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(Sell);
        }
        
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(Upgrade);
        }
    }
    
    public void UpdateMoney(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + money.ToString();
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives.ToString();
        }
    }
    
    public void UpdateWaveNumber(int wave)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + wave.ToString();
        }
    }
    
    public void UpdateWaveCountdown(float countdown)
    {
        if (countdownText != null)
        {
            countdownText.text = "Next Wave: " + Mathf.RoundToInt(countdown).ToString();
        }
    }
    
    public void ShowGameOverScreen()
    {
        gameOverUI.SetActive(true);
    }
    
    public void ShowWinScreen()
    {
        winUI.SetActive(true);
    }
    
    public void ShowNodeUI(Node node)
    {
        nodeUI.SetActive(true);
        
        Tower tower = node.tower.GetComponent<Tower>();
        
        if (sellValueText != null)
        {
            sellValueText.text = "$" + tower.GetSellValue().ToString();
        }
        
        if (tower.CanUpgrade())
        {
            upgradeButton.interactable = true;
            if (upgradeCostText != null)
            {
                upgradeCostText.text = "$" + tower.towerData.upgradeCost.ToString();
            }
        }
        else
        {
            upgradeButton.interactable = false;
            if (upgradeCostText != null)
            {
                upgradeCostText.text = "MAX";
            }
        }
    }
    
    public void HideNodeUI()
    {
        nodeUI.SetActive(false);
    }
    
    private void StartWave()
    {
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.StartNextWave();
        }
    }
    
    private void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    private void Sell()
    {
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SellTower();
        }
    }
    
    private void Upgrade()
    {
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.UpgradeTower();
        }
    }
}