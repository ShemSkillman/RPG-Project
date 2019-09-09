using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Combat;
using System.Collections.Generic;
using System.Collections;
using System;

namespace RPG.Control
{
    [RequireComponent(typeof(GroupMover))]
    public class GroupLeader : MonoBehaviour
    {        
        List<AIController> followers = new List<AIController>();
        Vector3 nextPos;
        GroupMover groupMover;
        Mover myMover;
        Fighter myFighter;
        ActionScheduler actionScheduler;

        public bool isIdle;

        private void Awake()
        {
            groupMover = GetComponent<GroupMover>();
            myMover = GetComponent<Mover>();
            myFighter = GetComponent<Fighter>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        private void Start()
        {
            nextPos = transform.position;
        }

        private void Update()
        {
            if (myFighter.GetTarget() != null)
            {
                SetFollowersIdle(false);
                print("attacking");
                GroupAttack();
            }
            else if (nextPos != myMover.GetNextPathCorner())
            {
                SetFollowersIdle(false);
                print("moving");
                UpdateFormation();
            }
            else
            {
                SetFollowersIdle(true);
                print("doing nothing...");
            }
        }

        private void GroupAttack()
        {
            foreach (AIController follower in followers)
            {
                follower.GetComponent<Fighter>().Attack(myFighter.GetTarget());
            }     
        }

        private void UpdateFormation()
        {
            if (followers.Count == 0) return;
            nextPos = myMover.GetNextPathCorner();

            Vector3[,] posMatrix = groupMover.CalculateFormation(followers.Count, nextPos);
            int followerIndex = 0;

            for (int i = 0; i < posMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < posMatrix.GetLength(1); j++, followerIndex++)
                {
                    if (posMatrix[i, j] == null) return;

                    Mover mover = followers[followerIndex].GetComponent<Mover>();
                    mover.MoveTo(posMatrix[i, j], 1f);
                }
            }
        }

        private void SetFollowersIdle(bool isIdle)
        {
            foreach(AIController follower in followers)
            {
                follower.SetIsIdle(isIdle);
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
    }
}
