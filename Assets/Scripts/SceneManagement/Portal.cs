﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using RPG.Control;
using RPG.Core;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D
        }

        [SerializeField] int sceneToLoadIndex = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeDuration = 1.5f;
        [SerializeField] float fadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            if (sceneToLoadIndex < 0)
            {
                Debug.LogError("Scene to load is not set for portal " + gameObject.name);
                yield break;
            }
            GetComponent<Collider>().enabled = false;
            DontDestroyOnLoad(gameObject);

            Fader fader = FindObjectOfType<Fader>();

            PlayerControl(false);
            yield return fader.FadeOut(fadeDuration);

            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoadIndex);
            PlayerControl(false);

            savingWrapper.Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            savingWrapper.Save();

            yield return new WaitForSeconds(fadeWaitTime);
            fader.FadeIn(fadeDuration);

            PlayerControl(true);
            Destroy(gameObject);
        }

        private void PlayerControl(bool isControl)
        {
            GameObject player = GameObject.FindWithTag("Player");
            PlayerController playerController = player.GetComponent<PlayerController>();
            ActionScheduler actionScheduler = player.GetComponent<ActionScheduler>();

            playerController.enabled = isControl;
            if (!isControl) actionScheduler.CancelCurrentAction();
        }

        private Portal GetOtherPortal()
        {
            Portal[] allPortals =  FindObjectsOfType<Portal>();
            foreach(Portal portal in allPortals)
            {
                if (portal != this &&
                    portal.destination == destination)
                {
                    return portal;
                }
            }

            return null;
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            if (otherPortal == null) return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            NavMeshAgent playerNavMeshAgent = player.GetComponent<NavMeshAgent>();
            playerNavMeshAgent.Warp(otherPortal.spawnPoint.position);
            player.transform.rotation = otherPortal.spawnPoint.rotation;
        }
    }
}
