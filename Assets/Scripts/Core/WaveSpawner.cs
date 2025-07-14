using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float timeBetweenWaves = 5f;
    public bool autoStart = true;
    
    private float countdown = 2f;
    private int waveIndex = 0;
    private bool waveInProgress = false;
    private int enemiesAlive = 0;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        if (spawnPoint == null) spawnPoint = GameManager.Instance.enemySpawnPoint;
    }
    
    private void Update()
    {
        if (GameManager.GameIsOver) return;
        if (waveInProgress)
        {
            if (enemiesAlive == 0) WaveCompleted();
            return;
        }
        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
        if (UIManager.Instance != null) UIManager.Instance.UpdateWaveCountdown(countdown);
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();
        waveInProgress = false;
        enemiesAlive = 0;
        waveIndex = 0;
        countdown = 2f;
    }
    
    private IEnumerator SpawnWave()
    {
        WaveData currentWave = GameManager.Instance.GetCurrentWave();
        if (currentWave == null)
        {
            GameManager.Instance.GameWin();
            yield break;
        }
        waveInProgress = true;
        if (UIManager.Instance != null) UIManager.Instance.UpdateWaveNumber(waveIndex + 1);
        foreach (EnemySpawn enemySpawn in currentWave.enemies)
        {
            for (int i = 0; i < enemySpawn.count; i++)
            {
                SpawnEnemy(enemySpawn.enemyData);
                yield return new WaitForSeconds(enemySpawn.spawnDelay);
            }
        }
    }
    
    private void SpawnEnemy(EnemyData enemyData)
    {
        GameObject enemyGO = Instantiate(enemyData.prefab, spawnPoint.position, spawnPoint.rotation);
        GameManager.Instance.RegisterEnemy(enemyGO); // GameManager에 적 등록

        Debug.Log($"Spawned enemy: {enemyGO.name} with tag: {enemyGO.tag} at position: {enemyGO.transform.position}");
        
        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        if (enemyScript != null) enemyScript.Initialize(enemyData);
        enemiesAlive++;
        
        Debug.Log($"Enemies alive: {enemiesAlive}");
    }
    
    public void EnemyDestroyed() 
    { 
        enemiesAlive--;
        Debug.Log($"Enemy destroyed! Enemies alive: {enemiesAlive}");
    }
    
    private void WaveCompleted()
    {
        waveInProgress = false;
        waveIndex++;
        GameManager.Instance.NextWave();
        Debug.Log("Wave " + waveIndex + " completed!");

        if (GameMetricsCollector.Instance != null) GameMetricsCollector.Instance.RecordWaveStarted(waveIndex);

        // AI 에이전트를 찾아 웨이브 완료 신호를 보냅니다.
        SimpleTowerDefenseAgent agent = FindFirstObjectByType<SimpleTowerDefenseAgent>();
        if (agent != null)
        {
            agent.OnWaveCompleted();
        }
    }
    
    public void StartNextWave()
    {
        if (!waveInProgress) countdown = 0f;
    }
}