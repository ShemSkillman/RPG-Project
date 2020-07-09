using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Combat;
using System.Collections.Generic;
using System.Collections;
using System;

namespace RPG.Control
{
    // Allows entity to have other entities under its command
    public class GroupLeader : MonoBehaviour
    {                
        [Header("Formation Configuration:")]
        [Range(1, 10)]
        [SerializeField] int unitCountToRowCountRatio = 1;
        [SerializeField] float spacing = 2f;

        // Highest priority
        const int priority = 3;

        // Cache references
        Mover myMover;
        Fighter myFighter;
        ActionScheduler actionScheduler;

        // State
        List<AIController> followers = new List<AIController>();
        Vector3 nextPos;
        Vector3[,] posMatrix;
        bool isControl = true;
        bool isMoving = false;

        private void Awake()
        {
            myMover = GetComponent<Mover>();
            myFighter = GetComponent<Fighter>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        private void OnEnable()
        {
            actionScheduler.onStartAction += GiveOrders;
            actionScheduler.onFinishAction += GiveOrders;
        }

        private void OnDisable()
        {
            actionScheduler.onStartAction -= GiveOrders;
            actionScheduler.onFinishAction -= GiveOrders;
        }

        private void Start()
        {
            nextPos = transform.position;
        }

        private void Update()
        {
            // Player to toggle group / individual actions
            if(Input.GetKeyDown(KeyCode.G))
            {
                isControl = !isControl;

                if (!isControl) CancelOrders();
            }

            GetInFormation();
        }

        // Propagates leader actions to followers
        private void GiveOrders()
        {
            if (!isControl || followers.Count == 0) return;
            
            // Stops updating formation
            isMoving = false;

            switch (actionScheduler.GetCurrentActionType())
            {
                case ActionType.Attack:
                    GroupAttack();
                    break;
                case ActionType.Move:
                    GroupMove();
                    break;
            }
        }

        // All followers attck leader target
        private void GroupAttack()
        {
            foreach (AIController follower in followers)
            {
                follower.GetComponent<Fighter>().StartAttackAction(myFighter.GetTarget(), myMover.GetSpeedFraction(), priority);
            }
        }

        private void GroupMove()
        {
            isMoving = true;
        }

        // Followers go back to behaving independantly
        private void CancelOrders()
        {
            isMoving = false;
            foreach (AIController follower in followers)
            {
                follower.GetComponent<ActionScheduler>().CancelCurrentAction();
            }
        }

        public void AddFollower(AIController follower)
        {
            followers.Add(follower);
        }

        public void RemoveFollower(AIController follower)
        {
            followers.Remove(follower);
        }
               
        // Dynamically updates formation as leader moves
        private void GetInFormation()
        {
            if (!isMoving) return;
            
            posMatrix = GetFormationPositions(transform.position);
            int unitIndex = 0;

            for (int i = 0; i < posMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < posMatrix.GetLength(1); j++, unitIndex++)
                {
                    if (unitIndex == followers.Count) return;

                    // Followers get in formation
                    Mover mover = followers[unitIndex].GetComponent<Mover>();
                    mover.StartMoveAction(posMatrix[i, j], myMover.GetSpeedFraction(), priority);
                    followers[unitIndex].SetGuardPosition(posMatrix[i, j]);
                }
            }
        }

        // Fills formation matrix with follower positions
        public Vector3[,] GetFormationPositions(Vector3 refPosition)
        {
            int unitsToPosition = followers.Count;
            Vector3[,] posMatrix = GetFormationDimensions();

            Vector3 currentPos;

            // Formation rows
            for (int i = 0; i < posMatrix.GetLength(0); i++)
            {
                // Spaced row structure behind leader
                Vector3 rowOffset = refPosition + -transform.forward * ((i + 1) * spacing);

                // Each row centralized
                Vector3 unitOffset = -transform.right * (spacing * ((Mathf.Min(unitsToPosition, posMatrix.GetLength(1)) - 1) / 2f));

                // Set starting point for row
                currentPos = rowOffset;
                currentPos += unitOffset;

                // Position units in current row
                for (int j = 0; j < posMatrix.GetLength(1); j++, unitsToPosition--)
                {
                    if (unitsToPosition == 0) return posMatrix;

                    posMatrix[i, j] = currentPos;

                    // Add space between units
                    currentPos += transform.right * spacing;
                }
            }

            return posMatrix;
        }

        // Creates formation based on unit count
        private Vector3[,] GetFormationDimensions()
        {
            int unitsPerRow, rowCount, unitCapacity;
            unitsPerRow = rowCount = unitCapacity = 0;

            while (followers.Count > unitCapacity)
            {
                rowCount++;
                unitsPerRow = unitCountToRowCountRatio * rowCount;
                unitCapacity = rowCount * unitsPerRow;
            }

            return new Vector3[rowCount, unitsPerRow];
        }

        // Visualize formation
        private void OnDrawGizmosSelected()
        {
            if (posMatrix == null) return;

            Gizmos.color = Color.red;
            for (int i = 0; i < posMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < posMatrix.GetLength(1); j++)
                {
                    if (posMatrix[i, j] == null) return;

                    Gizmos.DrawSphere(posMatrix[i, j], 0.5f);
                }
            }
        }
    }
}
