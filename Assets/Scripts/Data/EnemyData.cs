using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public GameObject prefab;
    
    [Header("Stats")]
    public float health;
    public float speed;
    public int reward;
    
    [Header("Enemy Type")]
    public EnemyType enemyType;
    
    [Header("Visual")]
    public Color enemyColor = Color.white;
}

public enum EnemyType
{
    Runner,
    Tanker
}