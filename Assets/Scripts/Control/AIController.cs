using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using RPG.Attributes;
using GameDevTV.Utils;

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
        GameObject player;
        Mover mover;
        Fighter fighter;
        Health health;
        ActionScheduler actionScheduler;

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        int currentWaypointIndex = 0;
        float timeSinceWaypointArrival = Mathf.Infinity;

        void Awake()
        {
            player = GameObject.FindWithTag("Player");
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
            guardPosition = new LazyValue<Vector3>(GetInitialGuardPosition);
        }

        private void Start()
        {
            guardPosition.ForceInit();
        }

        private Vector3 GetInitialGuardPosition()
        {
            return transform.position;
        }

        private void Update()
        {
            if (health.GetIsDead()) return;

            if (InAttackRangeOfPlayer() &&
                fighter.CanAttack(player))
            {
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer <= suspicionTime)
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
            timeSinceLastSawPlayer += Time.deltaTime;
            actionScheduler.CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0f;
            fighter.Attack(player.GetComponent<CombatTarget>());
        }

        private bool InAttackRangeOfPlayer()
        {
            float distanceToPlayer =  Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer <= chaseDistance;
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}

