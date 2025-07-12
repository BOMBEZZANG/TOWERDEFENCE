using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public int startingMoney = 100;
    public int startingLives = 20;
    public Transform enemySpawnPoint;
    public Transform enemyTarget;
    public List<WaveData> waves = new List<WaveData>();
    
    [Header("Current Game State")]
    public int currentMoney;
    public int currentLives;
    public int currentWaveIndex = 0;
    public bool gameOver = false;
    public bool gameWon = false;
    
    public static bool GameIsOver { get { return Instance.gameOver; } }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        currentMoney = startingMoney;
        currentLives = startingLives;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoney(currentMoney);
            UIManager.Instance.UpdateLives(currentLives);
        }
    }
    
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoney(currentMoney);
        }
    }
    
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateMoney(currentMoney);
            }
            return true;
        }
        return false;
    }
    
    public void LoseLife()
    {
        currentLives--;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLives(currentLives);
        }
        
        if (currentLives <= 0)
        {
            GameOver();
        }
    }
    
    public void GameOver()
    {
        gameOver = true;
        Debug.Log("Game Over!");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen();
        }
    }
    
    public void GameWin()
    {
        gameWon = true;
        Debug.Log("You Win!");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen();
        }
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void NextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Count)
        {
            GameWin();
        }
    }
    
    public WaveData GetCurrentWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            return waves[currentWaveIndex];
        }
        return null;
    }
}