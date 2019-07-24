using UnityEngine;
using RPG.Core;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    private Health target;
    [SerializeField] float speed = 10f;
    [Range(0f, 1f)]
    [SerializeField] float targetOffsetPadding = 0.75f;
    [SerializeField] bool isHoming;
    [SerializeField] bool isSmartAim;
    Vector3 targetOffset;
    int damage;

    public void SetTarget(Health target, int damage)
    {
        this.target = target;
        this.damage = damage;
        GenerateRandomTargetOffset();
        if (!isHoming) AimAtTarget();
    }

    private void AimAtTarget()
    {
        Vector3 aimPos = GetAim(target.transform.position, target.GetComponent<NavMeshAgent>()) + targetOffset;
        transform.LookAt(aimPos);
    }

    // Marksman skill determines number of aim iterations??
    private Vector3 GetAim(Vector3 targetPos, NavMeshAgent targetNav)
    {
        if (!isSmartAim) return targetPos;

        float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        float timeToReachTarget = distanceToTarget / speed;
        Vector3 futureTargetPos = target.transform.position + targetNav.velocity * timeToReachTarget;
        if (targetPos != futureTargetPos) 
        {
            return GetAim(futureTargetPos, targetNav);
        }
        else
        {
            return targetPos;
        }
    }

    private void GenerateRandomTargetOffset()
    {
        CapsuleCollider targetCollider = target.GetComponent<CapsuleCollider>();
        float randomX = Random.Range(-targetCollider.radius, targetCollider.radius) * targetOffsetPadding;
        float randomZ = Random.Range(-targetCollider.radius, targetCollider.radius) * targetOffsetPadding;
        float randomY = Random.Range(targetCollider.height * (1 - targetOffsetPadding), targetCollider.height * targetOffsetPadding);
        targetOffset = new Vector3(randomX, randomY, randomZ);
    }

    private Vector3 GetTargetCenter()
    {
        CapsuleCollider targetCollider = target.GetComponent<CapsuleCollider>();
        return target.transform.position + Vector3.up * targetCollider.height / 2;
    }

    private void Update()
    {
        if (target == null) return;
        if (isHoming && !target.GetIsDead())
        {
            transform.LookAt(GetTargetCenter());
        }
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Health>() != target) return;
        if (target.GetIsDead()) return;
        target.TakeDamage(damage);
        Destroy(gameObject);        
    }
}
