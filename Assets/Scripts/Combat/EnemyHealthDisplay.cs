using RPG.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Combat
{
    // Displays health of enemy targeted by the player
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
            CombatTarget target = playerFighter.GetTarget();

            if (target == null)
            {
                healthText.text = "Enemy: N/A";
            }
            else
            {
                Health targetHealth = target.GetComponent<Health>();
                healthText.text = string.Format("Enemy: {0}/{1}", targetHealth.GetHealthPoints(), targetHealth.GetMaxHealthPoints());
            }            
        }
    }
}
