using System;
using UnityEngine;

namespace RPG.Core
{
    // Prevents saving object from being destroyed between scenes
    public class PersistentObjectsSpawner : MonoBehaviour
    {
        [SerializeField] GameObject persistentObjectsPrefab;

        static bool hasSpawned = false;

        private void Awake()
        {
            if (hasSpawned) return;

            hasSpawned = true;
            SpawnPersistentObjects();
        }

        private void SpawnPersistentObjects()
        {
            GameObject persistentObject = Instantiate(persistentObjectsPrefab);
            DontDestroyOnLoad(persistentObject);
        }
    }
}

