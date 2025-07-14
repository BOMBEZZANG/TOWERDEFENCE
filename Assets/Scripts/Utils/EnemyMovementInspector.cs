using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyMovementInspector : MonoBehaviour
{
    [Header("Enemy Movement Monitoring")]
    [SerializeField] private bool enableInspector = true;
    [Space]
    
    [Header("Current Enemy Data (Read-Only)")]
    [SerializeField] private int totalActiveEnemies = 0;
    [SerializeField] private float averageMovementTime = 0f;
    [SerializeField] private float fastestEnemyTime = 0f;
    [SerializeField] private float slowestEnemyTime = 0f;
    [Space]
    
    [Header("Enemy Type Breakdown")]
    [SerializeField] private List<EnemyTypeInfo> enemyTypes = new List<EnemyTypeInfo>();
    
    private Dictionary<GameObject, EnemyMovementData> activeEnemies = new Dictionary<GameObject, EnemyMovementData>();
    
    [System.Serializable]
    public class EnemyTypeInfo
    {
        public string enemyType;
        public int count;
        public float averageTime;
        public float minTime;
        public float maxTime;
    }
    
    [System.Serializable]
    public class EnemyMovementData
    {
        public string enemyType;
        public float spawnTime;
        public float startMovementTime;
        public bool hasStartedMoving;
        public Vector3 startPosition;
        public Vector3 currentPosition;
        public float currentMovementTime;
    }
    
    private void Update()
    {
        if (!enableInspector) return;
        
        UpdateEnemyTracking();
        UpdateInspectorData();
    }
    
    private void UpdateEnemyTracking()
    {
        // Find all active enemies
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        // Remove destroyed enemies from tracking
        var enemiesToRemove = new List<GameObject>();
        foreach (var kvp in activeEnemies)
        {
            if (kvp.Key == null)
            {
                enemiesToRemove.Add(kvp.Key);
            }
        }
        foreach (var enemy in enemiesToRemove)
        {
            activeEnemies.Remove(enemy);
        }
        
        // Add new enemies to tracking
        foreach (Enemy enemy in allEnemies)
        {
            if (!activeEnemies.ContainsKey(enemy.gameObject))
            {
                var movementData = new EnemyMovementData
                {
                    enemyType = enemy.enemyData?.enemyName ?? "Unknown",
                    spawnTime = Time.time,
                    startMovementTime = 0f,
                    hasStartedMoving = false,
                    startPosition = enemy.transform.position,
                    currentPosition = enemy.transform.position,
                    currentMovementTime = 0f
                };
                activeEnemies[enemy.gameObject] = movementData;
            }
            
            // Update movement tracking
            var data = activeEnemies[enemy.gameObject];
            data.currentPosition = enemy.transform.position;
            
            // Check if enemy started moving
            if (!data.hasStartedMoving && Vector3.Distance(data.startPosition, data.currentPosition) > 0.1f)
            {
                data.hasStartedMoving = true;
                data.startMovementTime = Time.time;
            }
            
            // Update movement time
            if (data.hasStartedMoving)
            {
                data.currentMovementTime = Time.time - data.startMovementTime;
            }
        }
    }
    
    private void UpdateInspectorData()
    {
        totalActiveEnemies = activeEnemies.Count;
        
        if (totalActiveEnemies == 0)
        {
            averageMovementTime = 0f;
            fastestEnemyTime = 0f;
            slowestEnemyTime = 0f;
            enemyTypes.Clear();
            return;
        }
        
        // Calculate average, min, and max movement times
        var movingEnemies = activeEnemies.Values.Where(e => e.hasStartedMoving && e.currentMovementTime > 0).ToList();
        
        if (movingEnemies.Count > 0)
        {
            averageMovementTime = movingEnemies.Average(e => e.currentMovementTime);
            fastestEnemyTime = movingEnemies.Min(e => e.currentMovementTime);
            slowestEnemyTime = movingEnemies.Max(e => e.currentMovementTime);
        }
        else
        {
            averageMovementTime = 0f;
            fastestEnemyTime = 0f;
            slowestEnemyTime = 0f;
        }
        
        // Update enemy type breakdown
        enemyTypes.Clear();
        var typeGroups = activeEnemies.Values.GroupBy(e => e.enemyType);
        
        foreach (var group in typeGroups)
        {
            var movingInGroup = group.Where(e => e.hasStartedMoving && e.currentMovementTime > 0).ToList();
            
            var typeInfo = new EnemyTypeInfo
            {
                enemyType = group.Key,
                count = group.Count()
            };
            
            if (movingInGroup.Count > 0)
            {
                typeInfo.averageTime = movingInGroup.Average(e => e.currentMovementTime);
                typeInfo.minTime = movingInGroup.Min(e => e.currentMovementTime);
                typeInfo.maxTime = movingInGroup.Max(e => e.currentMovementTime);
            }
            
            enemyTypes.Add(typeInfo);
        }
    }
    
    // Public methods for external access
    public int GetActiveEnemyCount() => totalActiveEnemies;
    public float GetAverageMovementTime() => averageMovementTime;
    public Dictionary<string, float> GetEnemyTypeAverages()
    {
        var result = new Dictionary<string, float>();
        foreach (var type in enemyTypes)
        {
            result[type.enemyType] = type.averageTime;
        }
        return result;
    }
    
    public List<EnemyMovementData> GetActiveEnemyData()
    {
        return activeEnemies.Values.ToList();
    }
}