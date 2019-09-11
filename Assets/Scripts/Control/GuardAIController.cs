using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Control
{
    public class GuardAIController : AIController
    {
        [Header("Patrol Configuration")]
        [SerializeField] protected PatrolPath patrolPath;
        [SerializeField] protected float waypointTolerance = 1f;
        [SerializeField] protected float waypointDwellTime = 4f;
        [Range(0, 1)]
        [SerializeField] protected float patrolSpeedFraction = 0.2f;

        protected int currentWaypointIndex = 0;
        protected float timeSinceWaypointArrival = Mathf.Infinity;

        protected override void MovementBehaviour()
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

            mover.StartMoveAction(nextPosition, patrolSpeedFraction, priority);
        }

        protected bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint <= waypointTolerance;
        }

        protected Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        protected void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextWaypointIndex(currentWaypointIndex);
        }
    }
}

