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
        [SerializeField] float chaseDistance = 10f;
        [SerializeField] float suspicionTime = 5f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 4f;
        [Range(0, 1)]
        [SerializeField] float patrolSpeedFraction = 0.2f;

        // Cache references
        Mover mover;
        Fighter fighter;
        Health health;
        EntityManager entityManager;
        List<CombatTarget> enemies = new List<CombatTarget>();
        ActionScheduler actionScheduler;
        CombatTarget myCombatTarget;
        CombatTarget currentTarget;

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawEnemy = Mathf.Infinity;
        int currentWaypointIndex = 0;
        float timeSinceWaypointArrival = Mathf.Infinity;

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

        private void OnEnable()
        {
            entityManager.onUpdateEntities += SetEnemies;
        }

        private void OnDisable()
        {
            entityManager.onUpdateEntities -= SetEnemies;
        }


        private void Start()
        {
            guardPosition.ForceInit();
            SetEnemies();
        }

        private void SetEnemies()
        {
            enemies = entityManager.GetEnemies(myCombatTarget);
        }

        private Vector3 GetInitialGuardPosition()
        {
            return transform.position;
        }

        private void Update()
        {
            if (health.GetIsDead()) return;

            currentTarget = CheckProximity();
            if (currentTarget != null &&
                fighter.CanAttack(currentTarget))
            {
                AttackBehaviour(currentTarget.GetComponent<CombatTarget>());
            }
            else if (timeSinceLastSawEnemy <= suspicionTime)
            {
                SuspiciousBehaviour();
            }
            else
            {
                PatrolBehaviour();
            }
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceWaypointArrival += Time.deltaTime;
                    if (timeSinceWaypointArrival > waypointDwellTime)
                    {
                        CycleWaypoint();
                    }
                }
                else
                {
                    timeSinceWaypointArrival = 0f;
                }
                nextPosition = GetCurrentWaypoint();
            }

            mover.StartMoveAction(nextPosition, patrolSpeedFraction);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint <= waypointTolerance;
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextWaypointIndex(currentWaypointIndex);
        }

        private void SuspiciousBehaviour()
        {
            timeSinceLastSawEnemy += Time.deltaTime;
            actionScheduler.CancelCurrentAction();
        }

        private void AttackBehaviour(CombatTarget target)
        {
            timeSinceLastSawEnemy = 0f;
            fighter.Attack(target);
        }

        private CombatTarget CheckProximity()
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
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}

