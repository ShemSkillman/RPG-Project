using System;
using UnityEngine;

namespace RPG.Audio
{
    public class RandomAudioPlayer : MonoBehaviour
    {
        [SerializeField] AudioClip[] audioClips;
        AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayRandomAudio()
        {
            audioSource.clip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
            audioSource.Play();
        }
    }
}

