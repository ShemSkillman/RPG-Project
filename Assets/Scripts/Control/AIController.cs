using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using RPG.Attributes;
using GameDevTV.Utils;
using System.Collections.Generic;
using System;
using System.Collections;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] protected GroupLeader leader;
        [SerializeField] protected float sightRange = 10f;
        [SerializeField] protected float chaseDistance = 20f;
        [SerializeField] protected float reactionTime = 0.2f;
        [SerializeField] protected float suspicionTime = 5f;
        [Range(0f, 1f)]
        [SerializeField] protected float normalSpeedFraction = 0.8f;
        protected const int priority = 1;

        // Cache references
        protected Mover mover;
        protected Fighter fighter;
        protected Health health;
        protected EntityManager entityManager;
        protected ActionScheduler actionScheduler;
        protected CombatTarget myCombatTarget;

        [SerializeField] protected CombatTarget currentTarget;
        protected CombatTarget lostTarget;
        protected List<CombatTarget> enemies = new List<CombatTarget>();
        protected List<CombatTarget> allies = new List<CombatTarget>();
        protected float timeSinceLastSawEnemy = Mathf.Infinity;
        protected LazyValue<Vector3> guardPosition;
        protected bool isSchedulerFree = true;
        protected bool isDead = false;

        void Awake()
        {
            mover = GetComponent<Mover>();
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            myCombatTarget = GetComponent<CombatTarget>();
            actionScheduler = GetComponent<ActionScheduler>();
            entityManager = FindObjectOfType<EntityManager>();
            guardPosition = new LazyValue<Vector3>(GetInitialGuardPosition);
        }

        protected void OnEnable()
        {
            entityManager.onUpdateEntities += SetEnemiesAndAllies;
            health.onDie.AddListener(SetIsDead);
            myCombatTarget.onAttacked += DecideAttackAgainstInstigator;
        }

        protected void OnDisable()
        {
            entityManager.onUpdateEntities -= SetEnemiesAndAllies;
            health.onDie.RemoveListener(SetIsDead);
            myCombatTarget.onAttacked -= DecideAttackAgainstInstigator;
        }

        protected void Start()
        {
            if (leader != null) leader.AddFollower(this);

            guardPosition.ForceInit();
            SetEnemiesAndAllies();
            StartCoroutine(AIBrain());
        }

        protected void SetEnemiesAndAllies()
        {
            enemies = entityManager.GetEnemies(myCombatTarget);
            allies = entityManager.GetAllies(myCombatTarget);
        }

        protected Vector3 GetInitialGuardPosition()
        {
            return transform.position;
        }

        public void SetGuardPosition(Vector3 newPos)
        {
            guardPosition.value = newPos;
        }

        protected void SetIsDead()
        {
            isDead = true;
            actionScheduler.Freeze();
            
            if (leader != null) leader.RemoveFollower(this);
        }

        protected void Update()
        {
            Timers();
        }

        protected virtual void Timers()
        {
            timeSinceLastSawEnemy += Time.deltaTime;
        }

        IEnumerator AIBrain()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(reactionTime);

                if (!isSchedulerFree) continue;

                CheckCurrentTarget();
                CheckProximity();
                AssistAllies();

                if (currentTarget != null)
                {
                    timeSinceLastSawEnemy = 0f;
                    AttackBehaviour();
                }
                else if (IsIdle())
                {
                    IdleBehaviour();
                }
                else
                {
                    MovementBehaviour();
                }
            }
        }

        protected virtual bool IsIdle()
        {
            if (timeSinceLastSawEnemy <= suspicionTime) return true;

            lostTarget = null;
            return false;
        }

        protected virtual void MovementBehaviour()
        {            
            if (mover.IsAtLocation(guardPosition.value) ||
                (actionScheduler.GetCurrentActionType() == ActionType.Move && mover.GetDestination() == guardPosition.value)) return;

            CheckSchedulerFree(mover.StartMoveAction(guardPosition.value, normalSpeedFraction, priority));
        }

        protected void IdleBehaviour()
        {
            if (lostTarget != null) LocateDistantTarget(lostTarget);

            if (actionScheduler.GetCurrentActionType() == ActionType.Stop) return;

            CheckSchedulerFree(mover.StartStopAction(priority));
        }

        public void AttackBehaviour()
        {
            if (actionScheduler.GetCurrentActionType() == ActionType.Attack && currentTarget == fighter.GetTarget()) return;
            
            CheckSchedulerFree(fighter.StartAttackAction(currentTarget, normalSpeedFraction, priority));
        }

        protected void CheckSchedulerFree(bool isStartActionSuccessful)
        {
            isSchedulerFree = isStartActionSuccessful;

            if (!isStartActionSuccessful)
            {
                actionScheduler.onFinishAction += SchedulerFree;
            }
        }

        protected void SchedulerFree()
        {
            isSchedulerFree = true;
            actionScheduler.onFinishAction -= SchedulerFree;
        }

        protected void CheckCurrentTarget()
        {
            if (!fighter.CanAttack(currentTarget))
            {
                currentTarget = null;
            }
            else if (Vector3.Distance(transform.position, currentTarget.transform.position) > chaseDistance)
            {
                lostTarget = currentTarget;
                currentTarget = null;
            }
        }

        protected void CheckProximity()
        {
            if (enemies.Count < 1 || currentTarget != null) return;

            float closestEnemyDistance = sightRange;
            CombatTarget closestEnemy = null;

            foreach(CombatTarget enemy in enemies)
            {
                if (enemy.GetIsDead()) continue;

                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy <= closestEnemyDistance)
                {
                    closestEnemyDistance = distanceToEnemy;
                    closestEnemy = enemy;
                }
            }

            currentTarget = closestEnemy;
        }

        protected void AssistAllies()
        {
            if (currentTarget != null || allies.Count < 1) return;

            float closestEnemyDistance = Mathf.Infinity;
            CombatTarget closestEnemy = null;

            foreach(CombatTarget ally in allies)
            {
                if (ally.GetIsDead() || (leader != null && leader.gameObject == ally.gameObject)) continue;

                float distanceToAlly = Vector3.Distance(transform.position, ally.transform.position);

                if (distanceToAlly <= sightRange)
                {
                    CombatTarget allyTarget = ally.GetComponent<Fighter>().GetTarget();
                    if (allyTarget == null) continue;

                    if (Vector3.Distance(transform.position, allyTarget.transform.position) < closestEnemyDistance)
                    {
                        closestEnemy = allyTarget;
                    }
                }
            }

            currentTarget = closestEnemy;
        }

        private void DecideAttackAgainstInstigator(AttackReport attackReport)
        {
            DecideAttackAgainstEnemy(attackReport.instigator.GetComponent<CombatTarget>());
        }

        private void DecideAttackAgainstEnemy(CombatTarget enemy)
        {
            if (currentTarget == null ||
                Vector3.Distance(enemy.transform.position, transform.position) < Vector3.Distance(currentTarget.transform.position, transform.position))
            {
                currentTarget = enemy;
            }
        }

        public void LocateDistantTarget(CombatTarget target)
        {
            if (currentTarget != null ||
                Vector3.Distance(transform.position, target.transform.position) > chaseDistance) return;
            currentTarget = target;
        }

        // Called by Unity
        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);

        }
    }
}

