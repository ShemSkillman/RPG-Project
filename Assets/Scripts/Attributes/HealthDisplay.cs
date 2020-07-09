using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    // Displays player health current / max
    public class HealthDisplay : MonoBehaviour
    {
        Health health;
        Text healthText;

        private void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
            healthText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            health.onHealthChange += UpdateDispay;
        }

        private void OnDisable()
        {
            health.onHealthChange -= UpdateDispay;
        }

        private void Start()
        {
            UpdateDispay();
        }

        private void UpdateDispay()
        {
            healthText.text = string.Format("Health: {0}/{1}", health.GetHealthPoints(), health.GetMaxHealthPoints());
        }
    }
}
