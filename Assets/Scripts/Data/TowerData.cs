using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Basic Info")]
    public string towerName;
    public int cost;
    public GameObject prefab;
    public Sprite icon;
    
    [Header("Combat Stats")]
    public float damage;
    public float range;
    public float fireRate;
    public float turnSpeed = 10f;
    
    [Header("Upgrade Info")]
    public TowerData upgradeTo;
    public int upgradeCost;
    
    [Header("Tower Type")]
    public TowerType towerType;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
}

public enum TowerType
{
    MachineGun,
    Cannon
}