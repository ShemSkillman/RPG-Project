using UnityEngine;

namespace RPG.Core
{
    public class ActionMarker : MonoBehaviour
    {
        Transform follow;
        ActionScheduler actionScheduler;

        void Update()
        {
            if (follow != null)
            {
                transform.position = follow.position;
            }
        }

        public void SetMarker(ActionScheduler actionScheduler, Transform follow)
        {
            actionScheduler.onFinishAction += DestroyMarker;

            this.actionScheduler = actionScheduler;
            this.follow = follow;
        }

        public void DestroyMarker()
        {
            actionScheduler.onFinishAction -= DestroyMarker;
            Destroy(gameObject);
        }
    }
}

