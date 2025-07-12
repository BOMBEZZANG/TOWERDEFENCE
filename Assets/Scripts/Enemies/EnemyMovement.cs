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
        target = Waypoints.points[0];
    }
    
    private void Update()
    {
        if (target == null) return;
        
        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        
        if (Vector3.Distance(transform.position, target.position) <= distanceThreshold)
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
    
    private void EndPath()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReachEnd();
        }
    }
}