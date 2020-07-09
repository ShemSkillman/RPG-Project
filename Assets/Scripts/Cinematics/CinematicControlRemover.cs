using UnityEngine;
using UnityEngine.Playables;
using RPG.Core;
using RPG.Control;

namespace RPG.Cinematics
{
    // Removes player control during cinematic
    public class CinematicControlRemover : MonoBehaviour
    {
        private GameObject player;
        private ActionScheduler actionScheduler;
        private PlayerController playerController;
        private PlayableDirector playableDirector;

        private void Awake()
        {
            playableDirector = GetComponent<PlayableDirector>();
            player = GameObject.FindGameObjectWithTag("Player");
            actionScheduler = player.GetComponent<ActionScheduler>();
            playerController = player.GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (playableDirector.playOnAwake)
            {
                DisableControl(playableDirector);
            }
        }

        private void OnEnable()
        {
            playableDirector.played += DisableControl;
            playableDirector.stopped += EnableControl;
        }

        private void OnDisable()
        {
            playableDirector.played -= DisableControl;
            playableDirector.stopped -= EnableControl;
        }

        private void DisableControl(PlayableDirector pd)
        {
            Debug.Log("Player control disabled");
            actionScheduler.CancelCurrentAction();
            playerController.enabled = false;
        }

        private void EnableControl(PlayableDirector pd)
        {
            Debug.Log("Player control enabled");
            playerController.enabled = true;
        }
    }
}
