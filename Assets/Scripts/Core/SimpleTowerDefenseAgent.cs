using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Linq;

public class SimpleTowerDefenseAgent : Agent
{
    [Header("Debug")]
    public bool debugMode = true;
    
    [Header("Game References")]
    public TowerData[] availableTowers;
    public int maxObservableEnemies = 5;
    public int maxObservableNodes = 10;
    
    [Header("Performance Tracking")]
    private int episodeStartMoney;
    private int episodeStartLives;
    private float episodeStartTime;
    private int enemiesKilled;
    private int towersBuilt;
    private int wavesCompleted;
    
    private List<Node> buildableNodes;
    private int selectedTowerType = 0;
    
    public override void Initialize()
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: Initialize called");
        
        if (availableTowers == null || availableTowers.Length == 0)
        {
            Debug.LogError("SimpleTowerDefenseAgent: No tower data assigned!");
        }
        
        buildableNodes = FindObjectsByType<Node>(FindObjectsSortMode.None).ToList();
        if (debugMode) Debug.Log($"Found {buildableNodes.Count} buildable nodes");
    }
    
    public override void OnEpisodeBegin()
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: OnEpisodeBegin called");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ManualReset();
            episodeStartMoney = GameManager.Instance.currentMoney;
            episodeStartLives = GameManager.Instance.currentLives;
            episodeStartTime = Time.time;
            enemiesKilled = 0;
            towersBuilt = 0;
            wavesCompleted = 0;
        }
        
        selectedTowerType = 0;
        
        if (buildableNodes == null)
        {
            buildableNodes = FindObjectsByType<Node>(FindObjectsSortMode.None).ToList();
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        if (GameManager.Instance == null) return;
        
        // Game state observations (4 values)
        sensor.AddObservation((float)GameManager.Instance.currentMoney / 1000f); // Normalized money
        sensor.AddObservation((float)GameManager.Instance.currentLives / 20f);   // Normalized lives
        sensor.AddObservation((float)GameManager.Instance.currentWaveIndex / 10f); // Normalized wave
        sensor.AddObservation(WaveSpawner.Instance != null ? 
            (WaveSpawner.Instance.GetComponent<WaveSpawner>().enabled ? 1f : 0f) : 0f); // Wave active
            
        // Available tower info (availableTowers.Length * 3 values)
        foreach (var tower in availableTowers)
        {
            sensor.AddObservation((float)tower.cost / 100f);     // Normalized cost
            sensor.AddObservation(tower.damage / 50f);           // Normalized damage
            sensor.AddObservation(tower.range / 10f);            // Normalized range
        }
        
        // Buildable nodes status (maxObservableNodes * 4 values)
        for (int i = 0; i < maxObservableNodes; i++)
        {
            if (i < buildableNodes.Count && buildableNodes[i] != null)
            {
                Node node = buildableNodes[i];
                sensor.AddObservation(node.transform.position.x / 10f); // Normalized x position
                sensor.AddObservation(node.transform.position.z / 10f); // Normalized z position
                sensor.AddObservation(node.tower != null ? 1f : 0f);   // Has tower
                sensor.AddObservation(CanAffordTowerAt(node) ? 1f : 0f); // Can afford tower
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
        
        // Enemy positions (maxObservableEnemies * 3 values)
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < maxObservableEnemies; i++)
        {
            if (i < enemies.Length && enemies[i] != null)
            {
                Transform enemy = enemies[i].transform;
                sensor.AddObservation(enemy.position.x / 10f); // Normalized x position
                sensor.AddObservation(enemy.position.z / 10f); // Normalized z position
                sensor.AddObservation(enemies[i].GetCurrentHealth() / 100f); // Normalized health
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
        
        if (debugMode && Time.frameCount % 60 == 0) // Log every 60 frames
        {
            Debug.Log($"Observations - Money: {GameManager.Instance.currentMoney}, Lives: {GameManager.Instance.currentLives}, Wave: {GameManager.Instance.currentWaveIndex}, Enemies: {enemies.Length}");
        }
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (GameManager.Instance == null || GameManager.Instance.gameOver) return;
        
        int action = actionBuffers.DiscreteActions[0];
        
        // Action space:
        // 0: Do nothing
        // 1-N: Select tower type (N = availableTowers.Length)
        // (N+1)-(N+maxObservableNodes): Build selected tower on node
        // (N+maxObservableNodes+1)-(N+2*maxObservableNodes): Upgrade tower on node
        // (N+2*maxObservableNodes+1)-(N+3*maxObservableNodes): Sell tower on node
        
        bool actionTaken = false;
        
        if (action == 0)
        {
            // Do nothing - small negative reward to encourage action
            AddReward(-0.005f);
        }
        else if (action <= availableTowers.Length)
        {
            // Select tower type
            selectedTowerType = action - 1;
            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.SelectTowerToBuild(availableTowers[selectedTowerType]);
                actionTaken = true;
            }
        }
        else if (action <= availableTowers.Length + maxObservableNodes)
        {
            // Build tower
            int nodeIndex = action - availableTowers.Length - 1;
            if (TryBuildTowerAt(nodeIndex))
            {
                actionTaken = true;
                towersBuilt++;
                AddReward(0.1f); // Reward for building
            }
            else
            {
                AddReward(-0.05f); // Penalty for invalid build
            }
        }
        else if (action <= availableTowers.Length + 2 * maxObservableNodes)
        {
            // Upgrade tower
            int nodeIndex = action - availableTowers.Length - maxObservableNodes - 1;
            if (TryUpgradeTowerAt(nodeIndex))
            {
                actionTaken = true;
                AddReward(0.05f); // Reward for upgrading
            }
            else
            {
                AddReward(-0.5f); 
            }
        }
        else if (action <= availableTowers.Length + 3 * maxObservableNodes)
        {
            // Sell tower
            int nodeIndex = action - availableTowers.Length - 2 * maxObservableNodes - 1;
            if (TrySellTowerAt(nodeIndex))
            {
                actionTaken = true;
                AddReward(-0.02f); // Small penalty for selling (should be strategic)
            }
            else
            {
                AddReward(-0.01f); // Penalty for invalid sell
            }
        }
        
        // Check for episode end conditions
        CheckEpisodeEnd();
        
        if (debugMode)
        {
            if (actionTaken)
            {
                Debug.Log($"Action {action} executed successfully");
            }
            else if (action > 0)
            {
                Debug.Log($"Action {action} failed to execute");
            }
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Simple heuristic: Try to build towers when possible
        if (GameManager.Instance != null && buildableNodes.Count > 0)
        {
            var emptyNodes = buildableNodes.Where(n => n.tower == null).ToList();
            if (emptyNodes.Count > 0 && GameManager.Instance.currentMoney >= availableTowers[0].cost)
            {
                // Build first available tower type on first empty node
                discreteActionsOut[0] = availableTowers.Length + 1; // First build action
            }
            else
            {
                discreteActionsOut[0] = 0; // Do nothing
            }
        }
        else
        {
            discreteActionsOut[0] = 0; // Do nothing
        }
    }
    
    private bool CanAffordTowerAt(Node node)
    {
        if (selectedTowerType >= availableTowers.Length) return false;
        return GameManager.Instance.currentMoney >= availableTowers[selectedTowerType].cost;
    }
    
    private bool TryBuildTowerAt(int nodeIndex)
    {
        if (nodeIndex >= buildableNodes.Count || buildableNodes[nodeIndex] == null) return false;
        if (buildableNodes[nodeIndex].tower != null) return false;
        if (selectedTowerType >= availableTowers.Length) return false;
        
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectTowerToBuild(availableTowers[selectedTowerType]);
            return BuildManager.Instance.BuildTowerOn(buildableNodes[nodeIndex]);
        }
        return false;
    }
    
    private bool TryUpgradeTowerAt(int nodeIndex)
    {
        if (nodeIndex >= buildableNodes.Count || buildableNodes[nodeIndex] == null) return false;
        if (buildableNodes[nodeIndex].tower == null) return false;
        
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectNode(buildableNodes[nodeIndex]);
            BuildManager.Instance.UpgradeTower();
            return true;
        }
        return false;
    }
    
    private bool TrySellTowerAt(int nodeIndex)
    {
        if (nodeIndex >= buildableNodes.Count || buildableNodes[nodeIndex] == null) return false;
        if (buildableNodes[nodeIndex].tower == null) return false;
        
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectNode(buildableNodes[nodeIndex]);
            BuildManager.Instance.SellTower();
            return true;
        }
        return false;
    }
    
    private void CheckEpisodeEnd()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.gameOver)
        {
            // Game lost - negative reward based on performance
            float performance = CalculatePerformanceScore();
            AddReward(-1f + performance); // Base penalty reduced by performance
            Debug.Log($"Episode ended - Game Over! Performance: {performance}, Lives: {GameManager.Instance.currentLives}");
            EndEpisode();
        }
        else if (GameManager.Instance.gameWon)
        {
            // Game won - large positive reward
            float performance = CalculatePerformanceScore();
            AddReward(10f + performance); // Large win bonus plus performance
            Debug.Log($"Episode ended - Game Won! Performance: {performance}, Wave: {GameManager.Instance.currentWaveIndex}");
            EndEpisode();
        }
    }
    
    private float CalculatePerformanceScore()
    {
        if (GameManager.Instance == null) return 0f;
        
        float score = 0f;
        
        // Reward for keeping lives
        score += (float)GameManager.Instance.currentLives / episodeStartLives * 2f;
        
        // Reward for efficient money usage
        float moneyUsed = episodeStartMoney - GameManager.Instance.currentMoney;
        score += Mathf.Min(moneyUsed / episodeStartMoney, 1f) * 1f;
        
        // Reward for waves completed
        score += GameManager.Instance.currentWaveIndex * 0.5f;
        
        // Reward for enemies killed and towers built
        score += enemiesKilled * 0.01f;
        score += towersBuilt * 0.05f;
            // << 이 부분을 추가하세요 >>
    // Reward for waves completed
    score += wavesCompleted * 0.5f; // 웨이브를 깰 때마다 0.5점의 추가 보상!
    // << 여기까지 >>

        // Reward for time efficiency (longer survival = better)
        float episodeTime = Time.time - episodeStartTime;
        score += Mathf.Min(episodeTime / 300f, 1f) * 1f; // Max 5 minutes
        
        return score;
    }
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        AddReward(0.02f); // Small reward for each enemy killed
    }
    public void OnWaveCompleted()
    {
        wavesCompleted++;
        AddReward(0.5f); // 웨이브 클리어 시 보상을 즉시 줍니다.
    }
    private void Update()
    {
        // Always check for episode end conditions
        CheckEpisodeEnd();
        
        // Request decision periodically during training
        if (GameManager.Instance != null && !GameManager.Instance.gameOver && !GameManager.Instance.gameWon)
        {
            RequestDecision();
        }
    }
}