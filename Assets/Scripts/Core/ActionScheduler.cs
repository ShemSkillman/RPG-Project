using System;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;
        int currentActionPriority = 0;
        ActionType currentActionType = ActionType.None;

        public Action OnFinishAction, onStartAction;

        public bool StartAction(IAction action, int actionPriority, ActionType actionType)
        {
            if (action != null && actionPriority < currentActionPriority) return false;

            if (currentAction != null)
            {
                currentAction.Cancel();
            }

            currentAction = action;
            currentActionPriority = actionPriority;
            currentActionType = actionType;

            return true;
        }

        public void CancelCurrentAction()
        {
            StartAction(null, 0, ActionType.None);

            OnFinishAction?.Invoke();
        }

        public ActionType GetCurrentActionType()
        {
            return currentActionType;
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
