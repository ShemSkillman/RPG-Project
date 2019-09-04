using UnityEngine;
using UnityEngine.AI;
using RPG.Attributes;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [Range(0f, 1f)]
        [SerializeField] float targetOffsetPadding = 0.75f;
        [SerializeField] bool isHoming;
        [SerializeField] bool isSmartAim;
        [SerializeField] GameObject hitEffect;
        [SerializeField] float maxLifeTime = 10f;
        [SerializeField] GameObject[] destroyOnHit;
        [SerializeField] float lifeAfterImpact = 3f;
        public UnityEvent onHit;

        CombatTarget target;
        bool isStopped, missedTarget;
        Vector3 targetOffset;
        AttackPayload attackPayload;

        public void SetTarget(CombatTarget target, AttackPayload attackPayload)
        {
            this.target = target;
            this.attackPayload = attackPayload;

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
            if (other.gameObject.GetComponent<CombatTarget>() != target) return;
            if (target.GetIsDead() || isStopped == true || missedTarget) return;

            missedTarget = !target.HandleAttack(attackPayload);
            if (missedTarget) return;

            onHit.Invoke();
            isStopped = true;
            if (hitEffect != null) Instantiate(hitEffect, transform.position, transform.rotation);

            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }

            Destroy(gameObject, lifeAfterImpact);
        }
    }
}

