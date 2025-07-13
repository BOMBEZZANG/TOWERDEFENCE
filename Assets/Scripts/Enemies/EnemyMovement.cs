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
        Debug.Log($"Enemy {gameObject.name} starting movement with speed {speed}, target: {target?.name}");
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
            Debug.Log($"Enemy {gameObject.name} reached end of path");
            EndPath();
            return;
        }
        
        waypointIndex++;
        target = Waypoints.points[waypointIndex];
        Debug.Log($"Enemy {gameObject.name} moving to waypoint {waypointIndex}: {target.name}");
    }
    
    private void SetInitialTarget()
    {
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            target = Waypoints.points[0];
            Debug.Log($"Set initial target for {gameObject.name}: {target.name}, total waypoints: {Waypoints.points.Length}");
        }
        else
        {
            Debug.LogError($"No waypoints found! Enemy {gameObject.name} cannot move.");
        }
    }
    
    private void EndPath()
    {
        Debug.Log($"Enemy {gameObject.name} reached the end!");
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReachEnd();
        }
    }
}