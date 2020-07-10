using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;
        [SerializeField] int currentActionPriority = 0;
        [SerializeField] ActionType currentActionType = ActionType.None;

        // State
        bool isFrozen = false;
        List<QueuedAction> queuedActions;
        bool isQueued = false;

        // Event
        public Action onFinishAction, onStartAction;

        private void Start()
        {
            queuedActions = new List<QueuedAction>();
        }

        private void Update()
        {
            // Perform subsequent instructions
            if (tag == "Player" && Input.GetKey(KeyCode.LeftShift)) isQueued = true;
            else isQueued = false;
        }

        // One action processed at any one time
        public bool StartAction(IAction action, int actionPriority, ActionType actionType)
        {            
            if (isFrozen || (action != null && actionPriority < currentActionPriority)) return false;

            // Queue?
            if (currentAction != null && isQueued)
            {
                queuedActions.Add(new QueuedAction(action, null));
                return true;
            }

            // Displace lower action priority
            CancelCurrentAction();

            currentAction = action;
            currentActionPriority = actionPriority;
            currentActionType = actionType;

            return true;
        }

        // Resets scheduler to empty state
        public void CancelCurrentAction()
        {
            if (currentAction != null) currentAction.Cancel();

            currentAction = null;
            currentActionPriority = 0;
            currentActionType = ActionType.None;

            // Notifies scheduler free
            onFinishAction?.Invoke();
        }

        public ActionType GetCurrentActionType()
        {
            return currentActionType;
        }

        public void Freeze()
        {
            CancelCurrentAction();
            isFrozen = true;
        }
    }

    // To be completed
    internal class QueuedAction
    {
        public object data;
        public IAction action;

        public QueuedAction(IAction action, object data)
        {
            this.data = data;
            this.action = action;
        }
    }

    public enum ActionType
    {
        Attack,
        Move,
        Stop,
        None
    }
}
