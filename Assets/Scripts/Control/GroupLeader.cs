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
        int rowCount = 1;
        int unitsPerRow = 1;
        int unitCount = -1;
        bool isMoving;

        private void Awake()
        {
            myMover = GetComponent<Mover>();
            myFighter = GetComponent<Fighter>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        private void OnEnable()
        {
            actionScheduler.onStartAction += GiveOrders;
        }

        private void OnDisable()
        {
            actionScheduler.onStartAction -= GiveOrders;
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
                    Fighter fighter = follower.GetComponent<Fighter>();
                    fighter.StartAttackAction(fighter.GetTarget(), priority);
                }
            }
            else if (actionScheduler.GetCurrentActionType() == ActionType.Move)
            {
                print("moving");
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
            if (!isMoving || followers.Count == 0 || nextPos == myMover.GetNextPathCorner()) return;

            nextPos = myMover.GetNextPathCorner();
            posMatrix = CalculateFormation(followers.Count, nextPos);

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < unitsPerRow; j++)
                {
                    if (posMatrix[i, j] == null) return;

                    Mover mover = followers[i * unitsPerRow + j].GetComponent<Mover>();
                    mover.StartMoveAction(posMatrix[i, j], 1f, priority);
                    followers[i * unitsPerRow + j].SetGuardPosition(posMatrix[i, j]);
                }
            }
        }

        public Vector3[,] CalculateFormation(int unitsToPosition, Vector3 destination)
        {
            Vector3[,] posMatrix = GetFormationDimensions(unitsToPosition);

            Vector3 currentPos;

            for (int i = 0; i < rowCount; i++)
            {
                Vector3 rowOffset = destination + -transform.forward * ((i + 1) * spacing);
                Vector3 unitOffset = -transform.right * (spacing * ((Mathf.Min(unitsToPosition, unitsPerRow) - 1) / 2f));

                currentPos = rowOffset;
                currentPos += unitOffset;

                for (int j = 0; j < unitsPerRow; j++, unitsToPosition--)
                {
                    if (unitsToPosition == 0) return posMatrix;

                    posMatrix[i, j] = currentPos;

                    currentPos += transform.right * spacing;
                }
            }

            return posMatrix;
        }

        private Vector3[,] GetFormationDimensions(int unitCount)
        {
            if (this.unitCount == unitCount)
            {
                return new Vector3[rowCount, unitsPerRow];
            }

            this.unitCount = unitCount;
            unitsPerRow = rowCount = 0;
            int unitCapacity = 0;

            while (unitCount > unitCapacity)
            {
                rowCount++;
                unitsPerRow = unitCountToRowCountRatio * rowCount;
                unitCapacity = rowCount * unitsPerRow;
            }

            Vector3[,] formation = new Vector3[rowCount, unitsPerRow];
            return formation;
        }

        private void OnDrawGizmosSelected()
        {
            if (posMatrix == null) return;

            Gizmos.color = Color.red;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < unitsPerRow; j++)
                {
                    if (posMatrix[i, j] == null) return;

                    Gizmos.DrawSphere(posMatrix[i, j], 0.5f);
                }
            }
        }
    }
}
