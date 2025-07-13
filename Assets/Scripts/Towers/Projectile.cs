using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    
    public void Seek(Transform _target, float _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        Debug.Log($"Projectile created targeting {target.name} with {damage} damage at {speed} speed");
    }
    
    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }
    
    private void HitTarget()
    {
        Debug.Log($"Projectile hit target {target.name}");
        
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"Dealing {damage} damage to {enemy.name}");
            enemy.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning($"Target {target.name} has no Enemy component!");
        }
        
        Destroy(gameObject);
    }
}