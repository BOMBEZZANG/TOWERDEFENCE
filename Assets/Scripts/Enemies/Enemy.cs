using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Components")]
    public Image healthBar;
    public Transform healthBarUI;
    
    [HideInInspector]
    public EnemyData enemyData;
    
    private float currentHealth;
    private bool isDead = false;
    
    
    private void Start()
    {
        if (healthBarUI != null) healthBarUI.gameObject.SetActive(false);
    }
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = enemyData.health;
        transform.localScale = Vector3.one * 0.65f;
        if (healthBar != null) healthBar.fillAmount = 1f;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = enemyData.enemyColor;
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null) movement.speed = enemyData.speed;
        
    }
    
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        Debug.Log($"Enemy {gameObject.name} taking {damage} damage. Health: {currentHealth}/{enemyData.health}");
        
        currentHealth -= damage;
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / enemyData.health;
            if (healthBarUI != null) healthBarUI.gameObject.SetActive(true);
        }
        
        Debug.Log($"Enemy {gameObject.name} health after damage: {currentHealth}/{enemyData.health}");
        
        if (currentHealth <= 0) 
        {
            Debug.Log($"Enemy {gameObject.name} dying!");
            Die();
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log($"Enemy {gameObject.name} died! Giving {enemyData.reward} money reward.");
        
        GameManager.Instance.AddMoney(enemyData.reward);
        if (GameMetricsCollector.Instance != null) 
        {
            GameMetricsCollector.Instance.RecordEnemyKilled();
        }
        
        // Notify AI agent of enemy kill
        SimpleTowerDefenseAgent agent = FindFirstObjectByType<SimpleTowerDefenseAgent>();
        if (agent != null) agent.OnEnemyKilled();
        
        CleanupAndDestroy();
    }
    
    public void ReachEnd()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"Enemy {gameObject.name} reached target! Losing 1 life. Lives before: {GameManager.Instance.currentLives}");
        
        
        GameManager.Instance.LoseLife();

        Debug.Log($"Lives after enemy reached end: {GameManager.Instance.currentLives}");

        // Notify WaveSpawner of enemy reaching target
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.EnemyReachedTarget();
        }

            // << 이 부분을 추가하세요 >>
    SimpleTowerDefenseAgent agent = FindFirstObjectByType<SimpleTowerDefenseAgent>();
    if (agent != null)
    {
        agent.AddReward(-1.0f); // 적을 놓치면 즉시 큰 벌점을 줍니다!
    }
        CleanupAndDestroy();
    }

    private void CleanupAndDestroy()
    {
        Debug.Log($"Enemy {gameObject.name} cleaning up and destroying");
        
        GameManager.Instance.UnregisterEnemy(gameObject); // GameManager에서 자신을 제거
        
        if (WaveSpawner.Instance != null) 
        {
            WaveSpawner.Instance.EnemyDestroyed();
            Debug.Log($"Called EnemyDestroyed() on WaveSpawner");
        }
        else
        {
            Debug.LogError("WaveSpawner.Instance is null!");
        }
        
        Destroy(gameObject);
    }
    
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return enemyData.health; }
    public bool IsDead() { return isDead; }
}