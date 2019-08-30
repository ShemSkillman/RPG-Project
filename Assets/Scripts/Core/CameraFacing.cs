using UnityEngine;

namespace RPG.Core
{
    public class CameraFacing : MonoBehaviour
    {
        private void Start()
        {
            transform.forward = Camera.main.transform.forward;
        }

        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}

