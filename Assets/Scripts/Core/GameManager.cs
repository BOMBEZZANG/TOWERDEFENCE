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

    // 생성된 오브젝트를 추적하기 위한 리스트
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeTowers = new List<GameObject>();
    
    public static bool GameIsOver { get { return Instance.gameOver; } }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    // AI 훈련을 위한 최적화된 수동 리셋 메서드
    public void ManualReset()
    {
        // 리스트를 순회하며 즉시 파괴 (훨씬 빠름)
        for (int i = activeTowers.Count - 1; i >= 0; i--)
        {
            if(activeTowers[i] != null) Destroy(activeTowers[i]);
        }
        activeTowers.Clear();

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if(activeEnemies[i] != null) Destroy(activeEnemies[i]);
        }
        activeEnemies.Clear();
        
        foreach (var projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(projectile.gameObject);
        foreach (var node in FindObjectsByType<Node>(FindObjectsSortMode.None)) node.tower = null;

        currentMoney = startingMoney;
        currentLives = startingLives;
        currentWaveIndex = 0;
        gameOver = false;
        gameWon = false;
        
        if (WaveSpawner.Instance != null) WaveSpawner.Instance.ResetSpawner();
        if (BuildManager.Instance != null) BuildManager.Instance.DeselectNode();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoney(currentMoney);
            UIManager.Instance.UpdateLives(currentLives);
            UIManager.Instance.UpdateWaveNumber(1);
            UIManager.Instance.gameOverUI.SetActive(false);
            UIManager.Instance.winUI.SetActive(false);
        }
    }

    // 외부에서 적과 타워를 리스트에 추가/제거할 수 있도록 메서드 추가
    public void RegisterEnemy(GameObject enemy) { activeEnemies.Add(enemy); }
    public void UnregisterEnemy(GameObject enemy) { activeEnemies.Remove(enemy); }
    public void RegisterTower(GameObject tower) { activeTowers.Add(tower); }
    public void UnregisterTower(GameObject tower) { activeTowers.Remove(tower); }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (UIManager.Instance != null) UIManager.Instance.UpdateMoney(currentMoney);
    }
    
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            if (UIManager.Instance != null) UIManager.Instance.UpdateMoney(currentMoney);
            return true;
        }
        return false;
    }
    
    public void LoseLife()
    {
        currentLives--;
        Debug.Log($"Life lost! Current lives: {currentLives}");
        if (UIManager.Instance != null) UIManager.Instance.UpdateLives(currentLives);
        if (currentLives <= 0) 
        {
            Debug.Log("Lives reached 0 or below! Calling GameOver()");
            GameOver();
        }
    }
    
    public void GameOver()
    {
        gameOver = true;
        Debug.Log("GameOver() called! Player lost the game!");
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverScreen();
    }
    
    public void GameWin()
    {
        gameWon = true;
        Debug.Log("GameWin() called! Player won the game!");
        if (UIManager.Instance != null) UIManager.Instance.ShowWinScreen();
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void NextWave()
    {
        currentWaveIndex++;
        Debug.Log($"NextWave called! Current wave index: {currentWaveIndex}, Total waves: {waves.Count}");
        
        if (currentWaveIndex >= waves.Count) 
        {
            Debug.Log("All waves completed! Calling GameWin()");
            GameWin();
        }
    }
    
    public WaveData GetCurrentWave()
    {
        if (currentWaveIndex < waves.Count) return waves[currentWaveIndex];
        return null;
    }
}