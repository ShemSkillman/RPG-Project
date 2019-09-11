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

        [SerializeField] float maxSpeed = 5.66f;
        [SerializeField] float tolerance = 1f;
        bool isActive;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            navMeshAgent.enabled = !health.GetIsDead();
            UpdateAnimator();

            if (isActive && AtDestination())
            {
                MovementComplete();
            }
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;

            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }

        public bool StartMoveAction(Vector3 destination, float speedFraction, int actionPriority)
        {
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Move)) return false;

            MoveTo(destination, speedFraction);
            isActive = true;

            actionScheduler.onStartAction?.Invoke();
            return true;
        }

        public bool StartStopAction(int actionPriority)
        {
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Stop)) return false;

            navMeshAgent.isStopped = true;

            actionScheduler.onStartAction?.Invoke();
            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public Vector3 GetNextPathCorner()
        {
            Vector3[] corners = navMeshAgent.path.corners;
            
            if (corners.Length > 1) return corners[0];

            return transform.position;
        }       

        private bool AtDestination()
        {
            if (navMeshAgent.remainingDistance <= tolerance)
            {
                return true;
            }
            return false;
        }

        private void MovementComplete()
        {
            actionScheduler.CancelCurrentAction();
        }

        public Vector3 GetDestination()
        {
            return navMeshAgent.destination;
        }

        public void Cancel()
        {
            isActive = false;
            navMeshAgent.isStopped = true;
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3)state;
            navMeshAgent.Warp(position.ToVector());
        }
    }
}

