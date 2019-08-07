using UnityEngine;
using RPG.Saving;
using System;
using System.Collections;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        private SavingSystem savingSystem;
        [SerializeField] float fadeInTime = 0.5f;

        private void Awake()
        {
            StartCoroutine(LoadLastScene());
        }

        IEnumerator LoadLastScene()
        {
            savingSystem = GetComponent<SavingSystem>();
            yield return savingSystem.LoadLastScene(defaultSaveFile);

            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();

            yield return fader.FadeIn(fadeInTime);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Delete();
            }
        }

        private void Delete()
        {
            savingSystem.Delete(defaultSaveFile);
        }

        public void Save()
        {
            savingSystem.Save(defaultSaveFile);
        }

        public void Load()
        {
            savingSystem.Load(defaultSaveFile);
        }
    }
}
