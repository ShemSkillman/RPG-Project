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
        public Action onFinishAction, onStartAction;
        bool isFrozen = false;
        List<QueuedAction> queuedActions;
        bool isQueued = false;

        private void Start()
        {
            queuedActions = new List<QueuedAction>();
        }

        private void Update()
        {
            if (tag == "Player" && Input.GetKey(KeyCode.LeftShift)) isQueued = true;
            else isQueued = false;
        }

        public bool StartAction(IAction action, int actionPriority, ActionType actionType)
        {
            if (isFrozen || (action != null && actionPriority < currentActionPriority)) return false;

            if (currentAction != null && isQueued)
            {
                queuedActions.Add(new QueuedAction(action, null));
                return true;
            }

            CancelCurrentAction();

            currentAction = action;
            currentActionPriority = actionPriority;
            currentActionType = actionType;

            return true;
        }

        public void CancelCurrentAction()
        {
            if (currentAction != null) currentAction.Cancel();

            currentAction = null;
            currentActionPriority = 0;
            currentActionType = ActionType.None;

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
