using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    
    private Transform target;
    private int waypointIndex = 0;
    private float distanceThreshold = 0.4f;
    
    private void Start()
    {
        SetInitialTarget();
    }
    
    private void Update()
    {
        if (target == null) 
        {
            SetInitialTarget();
            return;
        }
        
        Vector3 direction = target.position - transform.position;
        
        
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= distanceThreshold)
        {
            GetNextWaypoint();
        }
    }
    
    private void GetNextWaypoint()
    {
        if (waypointIndex >= Waypoints.points.Length - 1)
        {
            EndPath();
            return;
        }
        
        waypointIndex++;
        target = Waypoints.points[waypointIndex];
    }
    
    private void SetInitialTarget()
    {
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            target = Waypoints.points[0];
        }
    }
    
    private void EndPath()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReachEnd();
        }
    }
}