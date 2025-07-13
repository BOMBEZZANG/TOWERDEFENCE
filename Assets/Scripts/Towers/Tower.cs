using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
    [Header("Tower Components")]
    public Transform firePoint;
    public Transform partToRotate;
    public Transform rangeDisplay;
    
    [HideInInspector]
    public TowerData towerData;
    
    private Transform target;
    private float fireCountdown = 0f;
    private bool isUpgraded = false;
    private int upgradeLevel = 0; // 업그레이드 레벨 추적용 변수 추가
    
    private void Start()
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
    }
    
    public void Initialize(TowerData data)
    {
        towerData = data;
        
        if (rangeDisplay != null)
        {
            rangeDisplay.localScale = Vector3.one * towerData.range * 2f;
        }

        InvokeRepeating("UpdateTarget", 0f, 0.5f); 
    }

    private void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        
        if (nearestEnemy != null && shortestDistance <= towerData.range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }
    
    private void Update()
    {
        if (target == null) return;
        
        LockOnTarget();
        
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / towerData.fireRate;
        }
        
        fireCountdown -= Time.deltaTime;
    }
    
    private void LockOnTarget()
    {
        if (partToRotate == null) return;
        
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * towerData.turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }
    
    private void Shoot()
    {
        if (towerData.projectilePrefab != null)
        {
            GameObject projectileGO = Instantiate(towerData.projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Seek(target, towerData.damage, towerData.projectileSpeed);
            }
        }
        else
        {
            Enemy enemyScript = target.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(towerData.damage);
            }
        }
    }
    
    public bool CanUpgrade()
    {
        // 업그레이드는 한 번만 가능하도록 isUpgraded 플래그를 유지합니다.
        return !isUpgraded && towerData.upgradeTo != null;
    }

    // AI 에이전트를 위한 업그레이드 레벨 반환 메서드
    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }
    
    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        
        towerData = towerData.upgradeTo;
        isUpgraded = true;
        upgradeLevel = 1; // 업그레이드 시 레벨 1로 설정
        
        transform.localScale = Vector3.one * 1.0f;
        
        if (rangeDisplay != null)
        {
            rangeDisplay.localScale = Vector3.one * towerData.range * 2f;
        }
    }
    
    public int GetSellValue()
    {
        return Mathf.RoundToInt(towerData.cost * 0.75f);
    }
    
    private void OnMouseEnter()
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(true);
        }
    }
    
    private void OnMouseExit()
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
    }
}