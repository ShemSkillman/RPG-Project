using UnityEngine;
using UnityEngine.AI;
using RPG.Resources;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        private Health target;
        [SerializeField] float speed = 10f;
        [Range(0f, 1f)]
        [SerializeField] float targetOffsetPadding = 0.75f;
        [SerializeField] bool isHoming;
        [SerializeField] bool isSmartAim;
        [SerializeField] GameObject hitEffect;
        [SerializeField] float maxLifeTime = 10f;
        [SerializeField] GameObject[] destroyOnHit;
        [SerializeField] float lifeAfterImpact = 3f;

        bool isStopped;
        Vector3 targetOffset;
        public int damage;
        GameObject instigator;

        public void SetTarget(Health target, GameObject instigator, int damage)
        {
            this.target = target;
            this.damage = damage;
            this.instigator = instigator;

            if (!isHoming) AimAtTarget();
            Destroy(gameObject, maxLifeTime);
        }

        private void AimAtTarget()
        {
            GenerateRandomTargetOffset();
            Vector3 aimPos = GetAim(target.transform.position, target.GetComponent<NavMeshAgent>()) + targetOffset;
            transform.LookAt(aimPos);
        }
        
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
            float randomHeight = Random.Range(targetCollider.height * (1 - targetOffsetPadding), targetCollider.height * targetOffsetPadding);
            targetOffset = new Vector3(0f, randomHeight, 0f);
        }

        private Vector3 GetTargetCenter()
        {
            CapsuleCollider targetCollider = target.GetComponent<CapsuleCollider>();
            return target.transform.position + Vector3.up * targetCollider.height / 2;
        }

        private void Update()
        {
            if (target == null || isStopped) return;
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
            if (isStopped == true) return;
            isStopped = true;
            target.TakeDamage(instigator, damage);
            if (hitEffect != null) Instantiate(hitEffect, transform.position, transform.rotation);

            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }

            Destroy(gameObject, lifeAfterImpact);
        }
    }
}

