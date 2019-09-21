using UnityEngine;
using Cinemachine;

namespace RPG.Audio
{
    public class BoomMic : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float yOffset = 1.35f;

        private void LateUpdate()
        {
            transform.position = new Vector3(target.transform.position.x, target.transform.position.y + yOffset, target.transform.position.z);
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}

