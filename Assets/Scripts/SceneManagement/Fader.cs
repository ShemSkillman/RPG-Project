using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private bool isFaderFree = true;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1f;
        }

        public IEnumerator FadeOut(float time)
        {
            isFaderFree = false;
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime / time;
                yield return null;
            }
            isFaderFree = true;
        }

        public IEnumerator FadeIn(float time)
        {
            isFaderFree = false;
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime / time;
                yield return null;
            }
            isFaderFree = true;
        }

        public bool GetFaderInUse()
        {
            return isFaderFree;
        }
    }
}
