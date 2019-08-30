using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Image healthBarForeground;
        [SerializeField] Canvas rootCanvas;
        [SerializeField] float maxVisibilityTime = 5f;
        Health health;

        float timeSinceHealthChange = Mathf.Infinity;

        private void Awake()
        {
            health = GetComponentInParent<Health>();
        }

        private void OnEnable()
        {
            health.onHealthChange.AddListener(HealthChange);
        }

        private void OnDisable()
        {
            health.onHealthChange.RemoveListener(HealthChange);
        }

        private void Start()
        {
            SetHealthBar();
        }

        private void Update()
        {
            timeSinceHealthChange += Time.deltaTime;

            if (health.GetIsDead() || timeSinceHealthChange > maxVisibilityTime)
            {
                rootCanvas.enabled = false;
            }
            else
            {
                rootCanvas.enabled = true;
            }
        }

        private void SetHealthBar()
        {
            float healthNormalized = health.GetHealthPoints() / (float)health.GetMaxHealthPoints();
            healthBarForeground.transform.localScale = new Vector2(healthNormalized, 1f);
        }

        public void HealthChange()
        {
            timeSinceHealthChange = 0f;
            SetHealthBar();
        }
    }
}

