using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Coroutine currentlyActiveFade;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Instant screen black
        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1f;
        }

        public Coroutine FadeOut(float time)
        {
            return Fade(1, time);
        }

        public Coroutine FadeIn(float time)
        {
            return Fade(0, time);
        }

        // Target = opacity of fade to reach
        // Time = time to reach target
        private Coroutine Fade(float target, float time)
        {
            // Prevent multiple fade
            if (currentlyActiveFade != null) StopCoroutine(currentlyActiveFade);
            currentlyActiveFade = StartCoroutine(FadeRoutine(target, time));
            return currentlyActiveFade;
        }

        private IEnumerator FadeRoutine(float target, float time)
        {
            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                // Increment toward target over time
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime / time);
                yield return null;
            }
        }

    }
}
