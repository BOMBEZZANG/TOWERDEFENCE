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
        if (healthBarUI != null)
        {
            healthBarUI.gameObject.SetActive(false);
        }
    }
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = enemyData.health;
        
        // Set enemy scale to 65px x 65px (assuming 1 unit = 100px, so 0.65f = 65px)
        transform.localScale = Vector3.one * 0.65f;
        
        if (healthBar != null)
        {
            healthBar.fillAmount = 1f;
        }
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = enemyData.enemyColor;
        }
        
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = enemyData.speed;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / enemyData.health;
            
            if (healthBarUI != null)
            {
                healthBarUI.gameObject.SetActive(true);
            }
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        GameManager.Instance.AddMoney(enemyData.reward);
        
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.EnemyDestroyed();
        }
        
        Destroy(gameObject);
    }
    
    public void ReachEnd()
    {
        if (isDead) return;
        
        isDead = true;
        GameManager.Instance.LoseLife();
        
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.EnemyDestroyed();
        }
        
        Destroy(gameObject);
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return enemyData.health;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}