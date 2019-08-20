using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Image healthBarForeground;
        Health health;

        private void Awake()
        {
            health = GetComponentInParent<Health>();
        }

        private void Update()
        {
            if (health.GetIsDead()) gameObject.SetActive(false);

            float healthNormalized = health.GetHealthPoints() / (float)health.GetMaxHealthPoints();
            healthBarForeground.transform.localScale = new Vector2(healthNormalized, 1f);
        }
    }
}

