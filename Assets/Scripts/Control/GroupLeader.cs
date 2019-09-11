using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Combat;
using System.Collections.Generic;
using System.Collections;
using System;

namespace RPG.Control
{
    public class GroupLeader : MonoBehaviour
    {        
        List<AIController> followers = new List<AIController>();
        Vector3 nextPos;
        Mover myMover;
        Fighter myFighter;
        ActionScheduler actionScheduler;
        const int priority = 3;

        [Header("Formation Configuration:")]
        [Range(1, 10)]
        [SerializeField] int unitCountToRowCountRatio = 1;
        [SerializeField] float spacing = 2f;

        Vector3[,] posMatrix;
        [SerializeField] bool isMoving;

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
            GetInFormation();
        }

        private void GiveOrders()
        {
            if (followers.Count == 0) return;

            isMoving = false;

            if (actionScheduler.GetCurrentActionType() == ActionType.Attack)
            {
                foreach (AIController follower in followers)
                {
                    follower.SetCurrentTarget(myFighter.GetTarget());
                }
            }
            else if (actionScheduler.GetCurrentActionType() == ActionType.Move)
            {
                isMoving = true;
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

                    Mover mover = followers[unitIndex].GetComponent<Mover>();
                    mover.StartMoveAction(posMatrix[i, j], 1f, priority);
                    followers[unitIndex].SetGuardPosition(posMatrix[i, j]);
                }
            }
        }

        public Vector3[,] GetFormationPositions(Vector3 refPosition)
        {
            int unitsToPosition = followers.Count;
            Vector3[,] posMatrix = GetFormationDimensions();

            Vector3 currentPos;

            for (int i = 0; i < posMatrix.GetLength(0); i++)
            {
                Vector3 rowOffset = refPosition + -transform.forward * ((i + 1) * spacing);
                Vector3 unitOffset = -transform.right * (spacing * ((Mathf.Min(unitsToPosition, posMatrix.GetLength(1)) - 1) / 2f));

                currentPos = rowOffset;
                currentPos += unitOffset;

                for (int j = 0; j < posMatrix.GetLength(1); j++, unitsToPosition--)
                {
                    if (unitsToPosition == 0) return posMatrix;

                    posMatrix[i, j] = currentPos;

                    currentPos += transform.right * spacing;
                }
            }

            return posMatrix;
        }

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
