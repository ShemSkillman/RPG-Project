using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System.Collections;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private Health health;
        ActionScheduler actionScheduler;
        Coroutine currentMoveAction;

        [SerializeField] float maxSpeed = 5.66f;

        // Max distance allowed from target location
        [SerializeField] float tolerance = 1f;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            // Can walk over dead characters
            navMeshAgent.enabled = !health.GetIsDead();
            UpdateAnimator();
        }

        IEnumerator MoveActionProgress()
        {
            while (!health.GetIsDead() && !AtDestination())
            {
                yield return null;
            }

            MovementComplete();
        }

        // Character move animation matched to forward speed
        private void UpdateAnimator()
        {
            Vector3 velocity = Vector3.zero;
            if (navMeshAgent.enabled) velocity = navMeshAgent.velocity;
            
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;

            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }

        // Decides if move instruction can be processed
        public bool StartMoveAction(Vector3 destination, float speedFraction, int actionPriority)
        {
            // Compare priority to current action priority in scheduler
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Move) || !navMeshAgent.enabled) return false;

            MoveTo(destination, speedFraction);

            // Override previous move instruction if required
            if (currentMoveAction != null) StopCoroutine(currentMoveAction);
            currentMoveAction = StartCoroutine(MoveActionProgress());

            // Occupy scheduler
            actionScheduler.onStartAction?.Invoke();
            return true;
        }

        // Decides if stop instruction can be processed
        public bool StartStopAction(int actionPriority)
        {
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Stop)) return false;

            if (navMeshAgent.enabled) navMeshAgent.isStopped = true;

            // Stop action must be overriden by action of equal or greater priority
            actionScheduler.onStartAction?.Invoke();
            return true;
        }

        // Initiates movement to desired location
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            if (!navMeshAgent.enabled) return;

            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        //public Vector3 GetNextPathCorner()
        //{
        //    Vector3[] corners = navMeshAgent.path.corners;
            
        //    if (corners.Length > 1) return corners[1];

        //    return transform.position;
        //}

        // Checks current movement action complete
        private bool AtDestination()
        {
            if (!navMeshAgent.enabled || navMeshAgent.pathPending) return false;

            // Prevent never reaching desitnation
            if (navMeshAgent.remainingDistance <= tolerance)
            {
                return true;
            }

            return false;
        }

        // Checks if at provided location
        public bool IsAtLocation(Vector3 location)
        {
            if (Vector3.Distance(transform.position, location) <= tolerance) return true;
            return false;
        }

        // Free scheduler
        private void MovementComplete()
        {
            actionScheduler.CancelCurrentAction();
        }

        public Vector3 GetDestination()
        {
            if (navMeshAgent.enabled)
            {
                return navMeshAgent.destination;
            }

            return transform.position;
        }

        public float GetSpeedFraction()
        {
            return (navMeshAgent.speed / maxSpeed);
        }

        // Stop movement
        public void Cancel()
        {
            if (currentMoveAction != null) StopCoroutine(currentMoveAction);
            Stop();
        }

        public void Stop()
        {
            if (navMeshAgent.enabled) navMeshAgent.isStopped = true;
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3)state;
            if (navMeshAgent.enabled) navMeshAgent.Warp(position.ToVector());
        }
    }
}

