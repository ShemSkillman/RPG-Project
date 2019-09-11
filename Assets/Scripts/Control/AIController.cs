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
        [SerializeField] GroupLeader leader;
        [SerializeField] protected float sightRange = 10f;
        [SerializeField] float reactionTime = 0.2f;
        [SerializeField] protected float suspicionTime = 5f;
        protected const int priority = 1;

        // Cache references
        protected Mover mover;
        protected Fighter fighter;
        protected Health health;
        protected EntityManager entityManager;
        protected ActionScheduler actionScheduler;
        protected CombatTarget myCombatTarget;

        protected CombatTarget currentTarget;
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
            timeSinceLastSawEnemy += Time.deltaTime;
        }

        IEnumerator AIBrain()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(reactionTime);

                if (!isSchedulerFree) continue;

                currentTarget = CheckProximity();
                CallForHelp();

                if (currentTarget != null)
                {
                    timeSinceLastSawEnemy = 0f;
                    AttackBehaviour();
                }
                else if (timeSinceLastSawEnemy <= suspicionTime)
                {
                    SuspiciousBehaviour();
                }
                else
                {
                    MovementBehaviour();
                }
            }
        }

        protected virtual void MovementBehaviour()
        {
            if (actionScheduler.GetCurrentActionType() == ActionType.Move && mover.GetDestination() == guardPosition.value) return;

            CheckSchedulerFree(mover.StartMoveAction(guardPosition.value, 1f, priority));
        }

        protected void SuspiciousBehaviour()
        {
            if (actionScheduler.GetCurrentActionType() == ActionType.Stop) return;

            CheckSchedulerFree(mover.StartStopAction(priority));
        }

        public void AttackBehaviour()
        {
            if (actionScheduler.GetCurrentActionType() == ActionType.Attack && currentTarget == fighter.GetTarget()) return;
            
            CheckSchedulerFree(fighter.StartAttackAction(currentTarget, priority));
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

        protected CombatTarget CheckProximity()
        {
            if (fighter.CanAttack(currentTarget)) return currentTarget;

            if (enemies.Count < 1) return null;

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

            return closestEnemy;
        }

        protected void CallForHelp()
        {
            if (currentTarget == null || allies.Count < 1) return;

            foreach(CombatTarget ally in allies)
            {
                if (ally.GetIsDead()) continue;

                float distanceToAlly = Vector3.Distance(transform.position, ally.transform.position);

                if (distanceToAlly <= sightRange)
                {
                    ally.GetComponent<AIController>()?.SetCurrentTarget(currentTarget);
                }
            }
        }

        private void DecideAttackAgainstInstigator(AttackReport attackReport)
        {
            Vector3 instigatorPos = attackReport.instigator.transform.position;
            if (currentTarget == null ||
                Vector3.Distance(instigatorPos, transform.position) < Vector3.Distance(currentTarget.transform.position, transform.position))
            {
                currentTarget = attackReport.instigator.GetComponent<CombatTarget>();
            }
        }

        public void SetCurrentTarget(CombatTarget target)
        {
            if (currentTarget != null) return;
            currentTarget = target;
        }

        // Called by Unity
        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
    }
}

