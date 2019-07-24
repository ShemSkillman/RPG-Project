using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        bool playedCinematic = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!playedCinematic && other.gameObject.tag == "Player")
            {
                playedCinematic = true;
                GetComponent<PlayableDirector>().Play();
            }            
        }
    }
}
