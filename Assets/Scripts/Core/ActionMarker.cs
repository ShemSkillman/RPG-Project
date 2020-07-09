using UnityEngine;

namespace RPG.Core
{
    // Shows player action in form of a marker
    public class ActionMarker : MonoBehaviour
    {
        Transform follow;
        ActionScheduler actionScheduler;

        void Update()
        {
            FollowEntity();
        }

        private void FollowEntity()
        {
            if (follow != null)
            {
                transform.position = follow.position;
            }
        }

        // Intialize
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

