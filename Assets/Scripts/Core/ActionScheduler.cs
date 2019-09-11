using System;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;
        [SerializeField] int currentActionPriority = 0;
        [SerializeField] ActionType currentActionType = ActionType.None;

        public Action onFinishAction, onStartAction;
        bool isFrozen = false;

        public bool StartAction(IAction action, int actionPriority, ActionType actionType)
        {
            if (isFrozen || (action != null && actionPriority < currentActionPriority)) return false;

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

    public enum ActionType
    {
        Attack,
        Move,
        Stop,
        None
    }
}
