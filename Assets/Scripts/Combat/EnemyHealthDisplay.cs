using RPG.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Combat
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        Fighter playerFighter;
        Text healthText;

        private void Awake()
        {
            playerFighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
            healthText = GetComponent<Text>();
        }

        private void Update()
        {
            Health targetHealth = playerFighter.GetTarget();

            if (targetHealth == null)
            {
                healthText.text = "Enemy: N/A";
            }
            else
            {
                healthText.text = string.Format("Enemy: {0}/{1}", targetHealth.GetHealthPoints(), targetHealth.GetMaxHealthPoints());
            }            
        }
    }
}
