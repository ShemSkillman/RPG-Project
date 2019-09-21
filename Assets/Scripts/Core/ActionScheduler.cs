using System;
using UnityEngine;
using System.Collections;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;
        [SerializeField] int currentActionPriority = 0;
        [SerializeField] ActionType currentActionType = ActionType.None;
        public ActionType cure {  get { return currentActionType; } private set { currentActionType = value; } }
        public ActionType cure2 { get; private set; }
        public Action onFinishAction, onStartAction;
        bool isFrozen = false;
        Queue actionsInQueue;
        

        public bool StartAction(ScheduledAction action)
        {
            if (isFrozen || (action.action != null && action.actionPriority < currentActionPriority)) return false;

            if (currentAction != null)
            {
                currentAction.Cancel();
                onFinishAction?.Invoke();
            }

            currentAction = action.action;
            currentActionPriority = action.actionPriority;
            currentActionType = action.actionType;

            return true;
        }

        public void CancelCurrentAction()
        {
            StartAction(new ScheduledAction());            
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

    public class ScheduledAction
    {
        public IAction action;
        public ActionType actionType;
        public int actionPriority;

        public ScheduledAction()
        {
            actionType = ActionType.None;
            actionPriority = 0;
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
