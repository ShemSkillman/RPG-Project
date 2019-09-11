using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using RPG.Attributes;
using GameDevTV.Utils;
using System.Collections.Generic;
using System;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] GroupLeader leader;
        [SerializeField] protected float chaseDistance = 10f;
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
        protected float timeSinceLastSawEnemy = Mathf.Infinity;
        protected LazyValue<Vector3> guardPosition;
        protected bool isSchedulerFree = true;

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
            entityManager.onUpdateEntities += SetEnemies;
        }

        protected void OnDisable()
        {
            entityManager.onUpdateEntities -= SetEnemies;
        }

        protected void Start()
        {
            if (leader != null) leader.AddFollower(this);

            guardPosition.ForceInit();
            SetEnemies();            
        }

        protected void SetEnemies()
        {
            enemies = entityManager.GetEnemies(myCombatTarget);
        }

        protected Vector3 GetInitialGuardPosition()
        {
            return transform.position;
        }

        public void SetGuardPosition(Vector3 newPos)
        {
            guardPosition.value = newPos;
        }

        protected void Update()
        {
            if (health.GetIsDead() || !isSchedulerFree) return;

            currentTarget = CheckProximity();

            if (currentTarget != null)
            {
                timeSinceLastSawEnemy = 0f;
                AttackBehaviour();
            }
            else if (timeSinceLastSawEnemy <= suspicionTime)
            {
                timeSinceLastSawEnemy += Time.deltaTime;
                SuspiciousBehaviour();
            }
            else
            {
                MovementBehaviour();
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

        protected void AttackBehaviour()
        {
            if (actionScheduler.GetCurrentActionType() == ActionType.Attack && currentTarget == fighter.GetTarget()) return;

            CheckSchedulerFree(fighter.StartAttackAction(currentTarget, priority));
        }

        protected void CheckSchedulerFree(bool isStartActionSuccessful)
        {
            isSchedulerFree = isStartActionSuccessful;

            if (!isStartActionSuccessful)
            {
                actionScheduler.OnFinishAction += SchedulerFree;
            }
        }

        protected void SchedulerFree()
        {
            isSchedulerFree = true;
            actionScheduler.OnFinishAction -= SchedulerFree;
        }

        protected CombatTarget CheckProximity()
        {
            if (fighter.CanAttack(currentTarget)) return currentTarget;

            if (enemies.Count < 1) return null;

            float closestEnemyDistance = chaseDistance;
            CombatTarget closestEnemy = null;

            foreach(CombatTarget character in enemies)
            {
                if (character.GetIsDead()) continue;

                float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);

                if (distanceToCharacter <= closestEnemyDistance)
                {
                    closestEnemyDistance = distanceToCharacter;
                    closestEnemy = character;
                }
            }

            return closestEnemy;
        }

        // Called by Unity
        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}

