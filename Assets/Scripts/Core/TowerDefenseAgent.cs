using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Linq;

public class TowerDefenseAgent : Agent
{
    [Header("Agent Settings")]
    public float episodeTimeout = 600f;
    public bool useHeuristic = false;
    
    [Header("Tower Types")]
    public TowerData basicTowerData;
    public TowerData slowTowerData;
    
    private GameManager gameManager;
    private BuildManager buildManager;
    private WaveSpawner waveSpawner;
    private Node[] buildNodes;
    
    private float episodeTimer;
    private int lastWaveIndex;
    private int lastLives;
    
    public override void Initialize()
    {
        gameManager = GameManager.Instance;
        buildManager = BuildManager.Instance;
        waveSpawner = WaveSpawner.Instance;
        
        buildNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        
        if (gameManager == null || buildManager == null || waveSpawner == null)
        {
            Debug.LogError("TowerDefenseAgent: Required managers not found!");
        }
        
        if (basicTowerData == null || slowTowerData == null)
        {
            Debug.LogError("TowerDefenseAgent: Tower data not assigned!");
        }
    }
    
    public override void OnEpisodeBegin()
    {
        episodeTimer = 0f;
        lastWaveIndex = 0;
        
        gameManager.ManualReset();

        lastLives = gameManager.currentLives;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        if(gameManager == null) return;

        sensor.AddObservation(gameManager.currentMoney / 1000f);
        sensor.AddObservation(gameManager.currentLives / (float)gameManager.startingLives);
        sensor.AddObservation(gameManager.currentWaveIndex / (float)gameManager.waves.Count);
        sensor.AddObservation(episodeTimer / episodeTimeout);
        
        foreach (Node node in buildNodes)
        {
            if (node.tower == null)
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
            else
            {
                Tower tower = node.tower.GetComponent<Tower>();
                if (tower.towerData.towerType == TowerType.MachineGun)
                {
                    sensor.AddObservation(1f);
                }
                else if (tower.towerData.towerType == TowerType.Cannon)
                {
                    sensor.AddObservation(2f);
                }
                else
                {
                    sensor.AddObservation(0f);
                }
                
                sensor.AddObservation(tower.GetUpgradeLevel() / 3f);
            }
        }
        
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int enemyCount = Mathf.Min(enemies.Length, 10);
        sensor.AddObservation(enemyCount / 10f);
        
        for (int i = 0; i < 10; i++)
        {
            if (i < enemies.Length)
            {
                Enemy enemy = enemies[i];
                if (enemy == null)
                {
                    sensor.AddObservation(0f); sensor.AddObservation(0f); sensor.AddObservation(0f);
                    continue;
                }
                Vector3 normalizedPos = enemy.transform.position / 20f;
                sensor.AddObservation(normalizedPos.x);
                sensor.AddObservation(normalizedPos.z);
                sensor.AddObservation(enemy.GetCurrentHealth() / enemy.GetMaxHealth());
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
        int actionType = actionBuffers.DiscreteActions[0];
        int towerType = actionBuffers.DiscreteActions[1];
        int nodeIndex = actionBuffers.DiscreteActions[2];
        
        if (nodeIndex >= buildNodes.Length)
        {
            AddReward(-0.05f);
            return;
        }
        
        Node selectedNode = buildNodes[nodeIndex];
        TowerData selectedTowerData = (towerType == 0) ? basicTowerData : slowTowerData;
        
        switch (actionType)
        {
            case 0: // Do Nothing
                break;
                
            case 1: // Build Tower
                if (selectedNode.tower == null && gameManager.currentMoney >= selectedTowerData.cost)
                {
                    buildManager.SelectTowerToBuild(selectedTowerData);
                    if (buildManager.BuildTowerOn(selectedNode))
                    {
                        AddReward(0.1f);
                    }
                    else { AddReward(-0.05f); }
                }
                else { AddReward(-0.05f); }
                break;
                
            case 2: // Upgrade Tower
                if (selectedNode.tower != null)
                {
                    Tower tower = selectedNode.tower.GetComponent<Tower>();
                    if (tower.CanUpgrade() && gameManager.SpendMoney(tower.towerData.upgradeCost))
                    {
                        tower.Upgrade();
                        AddReward(0.15f);
                    }
                    else { AddReward(-0.05f); }
                }
                else { AddReward(-0.05f); }
                break;
        }
    }
    
    private void CheckForRewards()
    {
        if (gameManager.currentWaveIndex > lastWaveIndex)
        {
            AddReward(1.0f);
            lastWaveIndex = gameManager.currentWaveIndex;
        }
        
        if (gameManager.currentLives < lastLives)
        {
            AddReward(-0.2f * (lastLives - gameManager.currentLives));
            lastLives = gameManager.currentLives;
        }

        if (gameManager.gameOver)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        
        if (gameManager.gameWon)
        {
            SetReward(2.0f);
            EndEpisode();
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        discreteActionsOut[2] = 0;
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            discreteActionsOut[0] = 1;
            discreteActionsOut[1] = 0;
            discreteActionsOut[2] = Random.Range(0, buildNodes.Length);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            discreteActionsOut[0] = 1;
            discreteActionsOut[1] = 1;
            discreteActionsOut[2] = Random.Range(0, buildNodes.Length);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            discreteActionsOut[0] = 2;
            discreteActionsOut[2] = Random.Range(0, buildNodes.Length);
        }
    }
    
    // Update()를 FixedUpdate()로 변경하고 RequestDecision()을 제거했습니다.
    private void FixedUpdate()
    {
        episodeTimer += Time.fixedDeltaTime;
        
        if (episodeTimer >= episodeTimeout)
        {
            SetReward(-0.5f);
            EndEpisode();
        }
        
        CheckForRewards();
    }
}