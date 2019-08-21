using UnityEngine;

namespace RPG.Core
{
    public class CameraFacing : MonoBehaviour
    {
        private void Start()
        {
            transform.forward = Camera.main.transform.forward;
        }

        void Update()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}

