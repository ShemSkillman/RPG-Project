using System;
using UnityEngine;

namespace RPG.Audio
{
    public class RandomAudioPlayer : MonoBehaviour
    {
        [SerializeField] AudioClip[] audioClips;
        AudioSource audioSource;
        const float pitchVariation = 0.2f;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayRandomAudio()
        {
            audioSource.clip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
            audioSource.pitch = UnityEngine.Random.Range(1 - 0.2f, 1 + 0.2f);
            audioSource.Play();
        }
    }
}

