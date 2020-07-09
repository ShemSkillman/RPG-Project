using RPG.Core;
using UnityEngine;

namespace RPG.Control
{
    // Currently the only variation of the stock AI controller
    public class GuardAIController : AIController
    {
        [Header("Patrol Configuration")]
        [SerializeField] protected PatrolPath patrolPath;
        [SerializeField] protected float waypointDwellTime = 4f;
        [Range(0, 1)]
        [SerializeField] protected float patrolSpeedFraction = 0.2f;
        [SerializeField] protected float waypointTolerance = 0.1f;

        // State
        protected int currentWaypointIndex = 0;
        protected float timeOnWaypoint = Mathf.Infinity;

        protected override void Timers()
        {
            base.Timers();

            if (patrolPath != null) timeOnWaypoint += Time.deltaTime;
        }

        protected override bool IsIdle()
        {
            if (patrolPath == null) return base.IsIdle();

            return base.IsIdle() || (AtWaypoint() && timeOnWaypoint <= waypointDwellTime);
        }

        protected override void MovementBehaviour()
        {
            if (patrolPath == null)
            {
                base.MovementBehaviour();
                return;
            }

            if (actionScheduler.GetCurrentActionType() == ActionType.Move && mover.GetDestination() == guardPosition.value) return;

            if (!AtWaypoint())
            {
                timeOnWaypoint = 0f;
            }
            else if(timeOnWaypoint > waypointDwellTime)
            {
                CycleWaypoint();
            }

            CheckSchedulerFree(mover.StartMoveAction(GetCurrentWaypoint(), patrolSpeedFraction, priority));
        }

        protected Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        protected void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextWaypointIndex(currentWaypointIndex);
        }

        public bool AtWaypoint()
        {
            Vector3 guardPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 waypointPos = new Vector3(GetCurrentWaypoint().x, 0f, GetCurrentWaypoint().z);

            if (Vector3.Distance(guardPos, waypointPos) <= waypointTolerance) return true;

            return false;
        }
    }
}

