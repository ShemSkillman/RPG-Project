using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Resources
{
    public class HealthDisplay : MonoBehaviour
    {
        Health health;
        Text healthText;

        private void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
            healthText = GetComponent<Text>();
        }

        private void Update()
        {
            healthText.text = string.Format("Health: {0}/{1}", health.GetHealthPoints(), health.GetMaxHealthPoints());
        }
    }
}
