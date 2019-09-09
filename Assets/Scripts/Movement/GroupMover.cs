using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Movement
{
    public class GroupMover : MonoBehaviour
    {
        [Header("Formation Configuration:")]
        [Range(1, 10)]
        [SerializeField] int unitCountToRowCountRatio = 1;
        [SerializeField] float spacing = 2f;
        Vector3[,] posMatrix;

        int rowCount = 1;
        int unitsPerRow = 1;
        int unitCount = -1;

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

