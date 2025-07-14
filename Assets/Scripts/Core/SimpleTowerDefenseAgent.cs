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
    public int maxObservableEnemies = 15;
    public int maxObservableNodes = 2;

    [Header("Performance Tracking")]
    private int episodeStartMoney;
    private int episodeStartLives;
    private float episodeStartTime;
    private int enemiesKilled;
    private int towersBuilt;
    private int wavesCompleted;
    private List<TowerType> builtTowerTypesThisEpisode;
    private float lastTowerBuildTime;

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
        lastTowerBuildTime = 0f;

        if (buildableNodes == null)
        {
            buildableNodes = FindObjectsByType<Node>(FindObjectsSortMode.None).ToList();
        }

        if (builtTowerTypesThisEpisode == null) builtTowerTypesThisEpisode = new List<TowerType>();
        builtTowerTypesThisEpisode.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (GameManager.Instance == null) return;

        sensor.AddObservation((float)GameManager.Instance.currentMoney / 1000f);
        sensor.AddObservation((float)GameManager.Instance.currentLives / 20f);
        sensor.AddObservation((float)GameManager.Instance.currentWaveIndex / 10f);
        sensor.AddObservation(WaveSpawner.Instance != null ? (WaveSpawner.Instance.GetComponent<WaveSpawner>().enabled ? 1f : 0f) : 0f);

        foreach (var tower in availableTowers)
        {
            sensor.AddObservation((float)tower.cost / 100f);
            sensor.AddObservation(tower.damage / 50f);
            sensor.AddObservation(tower.range / 10f);
        }

        for (int i = 0; i < maxObservableNodes; i++)
        {
            if (i < buildableNodes.Count && buildableNodes[i] != null)
            {
                Node node = buildableNodes[i];
                sensor.AddObservation(node.transform.position.x / 10f);
                sensor.AddObservation(node.transform.position.z / 10f);
                sensor.AddObservation(node.tower != null ? 1f : 0f);
                sensor.AddObservation(CanAffordTowerAt(node) ? 1f : 0f);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < maxObservableEnemies; i++)
        {
            if (i < enemies.Length && enemies[i] != null)
            {
                Transform enemy = enemies[i].transform;
                sensor.AddObservation(enemy.position.x / 10f);
                sensor.AddObservation(enemy.position.z / 10f);
                sensor.AddObservation(enemies[i].GetCurrentHealth() / 100f);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (GameManager.Instance == null || GameManager.Instance.gameOver) return;

        int action = actionBuffers.DiscreteActions[0];
        bool actionTaken = false;

        // Do nothing action
        if (action == 0)
        {
            // Check if we have any towers built
            bool hasTowers = buildableNodes.Any(n => n != null && n.tower != null);
            
            // Check if enemies are approaching and we have no defense
            var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            bool enemiesPresent = enemies != null && enemies.Length > 0;
            
            if (!hasTowers && enemiesPresent)
            {
                AddReward(-0.1f); // Penalty for doing nothing when enemies are present and no towers exist
            }
            else if (!hasTowers)
            {
                AddReward(-0.05f); // Moderate penalty for having no towers
            }
            else
            {
                AddReward(-0.001f); // Very small penalty for doing nothing when towers exist
            }
        }
        // Select tower type
        else if (action <= availableTowers.Length)
        {
            selectedTowerType = action - 1;
            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.SelectTowerToBuild(availableTowers[selectedTowerType]);
                actionTaken = true;
                AddReward(0.01f);
            }
        }
        // Build tower
        else if (action <= availableTowers.Length + maxObservableNodes)
        {
            int nodeIndex = action - availableTowers.Length - 1;
            if (TryBuildTowerAt(nodeIndex))
            {
                actionTaken = true;
                towersBuilt++;
                lastTowerBuildTime = Time.time;
                
                // Reward building towers, with bonus for early building
                float timeSinceStart = Time.time - episodeStartTime;
                float earlyBuildingBonus = Mathf.Max(0, (30f - timeSinceStart) / 30f) * 0.1f;
                AddReward(0.3f + earlyBuildingBonus);
            }
            else
            {
                AddReward(-0.02f); // Penalty for failed build attempts
            }
        }
        // Upgrade tower
        else if (action <= availableTowers.Length + 2 * maxObservableNodes)
        {
            int nodeIndex = action - availableTowers.Length - maxObservableNodes - 1;
            if (TryUpgradeTowerAt(nodeIndex))
            {
                actionTaken = true;
                AddReward(0.4f); // Upgrading is good strategy
            }
            else
            {
                AddReward(-0.02f); // Penalty for failed upgrade attempts
            }
        }
        // Sell tower
        else if (action <= availableTowers.Length + 3 * maxObservableNodes)
        {
            int nodeIndex = action - availableTowers.Length - 2 * maxObservableNodes - 1;
            if (TrySellTowerAt(nodeIndex))
            {
                actionTaken = true;
                // Heavy penalty for selling towers - only do this in very specific circumstances
                AddReward(-2.0f);
            }
            else
            {
                AddReward(-0.02f); // Penalty for failed sell attempts
            }
        }

        CheckEpisodeEnd();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (GameManager.Instance == null || buildableNodes == null || availableTowers == null || availableTowers.Length == 0)
        {
            discreteActionsOut[0] = 0; // Do nothing
            return;
        }

        var emptyNodes = buildableNodes.Where(n => n != null && n.tower == null).ToList();
        var occupiedNodes = buildableNodes.Where(n => n != null && n.tower != null).ToList();

        if (emptyNodes.Count > 0 && GameManager.Instance.currentMoney >= availableTowers[0].cost)
        {
            int targetNodeIndex = -1;
            for (int i = 0; i < buildableNodes.Count; i++)
            {
                if (buildableNodes[i] != null && buildableNodes[i].tower == null)
                {
                    targetNodeIndex = i;
                    break;
                }
            }

            if (targetNodeIndex >= 0)
            {
                selectedTowerType = 0;
                discreteActionsOut[0] = availableTowers.Length + 1 + targetNodeIndex;
                if (debugMode) Debug.Log($"Heuristic: Building tower on node {targetNodeIndex}");
                return;
            }
        }

        if (occupiedNodes.Count > 0 && GameManager.Instance.currentMoney >= 50)
        {
            for (int i = 0; i < buildableNodes.Count; i++)
            {
                if (buildableNodes[i] != null && buildableNodes[i].tower != null)
                {
                    Tower towerScript = buildableNodes[i].tower.GetComponent<Tower>();
                    if (towerScript != null && towerScript.CanUpgrade() &&
                        GameManager.Instance.currentMoney >= towerScript.towerData.upgradeCost)
                    {
                        discreteActionsOut[0] = availableTowers.Length + maxObservableNodes + 1 + i;
                        if (debugMode) Debug.Log($"Heuristic: Upgrading tower on node {i}");
                        return;
                    }
                }
            }
        }

        discreteActionsOut[0] = 0;
        if (debugMode) Debug.Log("Heuristic: No actions available");
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
        if (availableTowers == null || availableTowers.Length == 0) return false;

        if (selectedTowerType >= availableTowers.Length) selectedTowerType = 0;

        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectTowerToBuild(availableTowers[selectedTowerType]);
            bool result = BuildManager.Instance.BuildTowerOn(buildableNodes[nodeIndex]);

            if (result)
            {
                TowerData builtTowerData = availableTowers[selectedTowerType];
                if (!builtTowerTypesThisEpisode.Contains(builtTowerData.towerType))
                {
                    builtTowerTypesThisEpisode.Add(builtTowerData.towerType);
                    AddReward(0.4f);
                    Debug.Log($"Built a new type of tower: {builtTowerData.towerType}. Rewarding diversity!");
                }
            }
            return result;
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
            float performance = CalculatePerformanceScore();
            AddReward(-5f + performance);
            EndEpisode();
        }
        else if (GameManager.Instance.gameWon)
        {
            float performance = CalculatePerformanceScore();
            AddReward(10f + performance);
            EndEpisode();
        }
    }

    private float CalculatePerformanceScore()
    {
        if (GameManager.Instance == null) return 0f;
        float score = 0f;
        score += (float)GameManager.Instance.currentLives / episodeStartLives * 2f;
        score += GameManager.Instance.currentWaveIndex * 0.5f;
        score += enemiesKilled * 0.01f;
        score += towersBuilt * 0.05f;
        score += wavesCompleted * 0.5f;
        float episodeTime = Time.time - episodeStartTime;
        score += Mathf.Min(episodeTime / 300f, 1f) * 1f;
        return score;
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
        AddReward(0.1f);
    }

    public void OnWaveCompleted()
    {
        wavesCompleted++;
        AddReward(0.5f);
    }

    private void Update()
    {
        CheckEpisodeEnd();
        if (GameManager.Instance != null && !GameManager.Instance.gameOver && !GameManager.Instance.gameWon)
        {
            RequestDecision();
            
            // Give small periodic reward for maintaining towers
            if (Time.time - lastTowerBuildTime > 1f)
            {
                int towerCount = buildableNodes.Count(n => n != null && n.tower != null);
                if (towerCount > 0)
                {
                    AddReward(towerCount * 0.001f); // Small reward for each tower maintained
                }
            }
        }
    }
}