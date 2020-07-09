using UnityEngine;
using RPG.Combat;
using RPG.Attributes;

namespace RPG.UI.DamageText
{
    public class AttackTextSpawner : MonoBehaviour
    {
        
        [SerializeField] AttackText damageText;
        // Time before fading 
        [SerializeField] float textMaxIdleTime = 1f;

        // State
        AttackText currentInstance;
        float textIdleTime = 0f;
        CombatTarget combatTarget;

        private void Awake()
        {
            combatTarget = GetComponentInParent<CombatTarget>();
        }

        private void OnEnable()
        {
            combatTarget.onAttacked += Spawn;
        }

        private void OnDisable()
        {
            combatTarget.onAttacked -= Spawn;
        }

        private void Update()
        {
            AttackTextLifeTime();
        }

        // Monitors time left on attack text
        private void AttackTextLifeTime()
        {
            if (currentInstance != null)
            {
                textIdleTime += Time.deltaTime;

                if (textIdleTime > textMaxIdleTime)
                    TextFadeOut();
            }
        }

        // Lengthens and updates present attack text
        // Otherwise spawns new attack text
        public void Spawn(AttackReport attackReport)
        {
            textIdleTime = 0f;

            // Update current attack text if present
            if (currentInstance != null)
            {
                currentInstance.ExtractReport(attackReport);
                return;
            }

            // New attack text
            currentInstance = Instantiate(damageText, transform);
            currentInstance.ExtractReport(attackReport);
        }

        // Attack text discarded
        // Must instantiate new attack text when needed
        private void TextFadeOut()
        {
            currentInstance.GetComponent<Animator>().SetTrigger("fadeOut"); ;
            currentInstance = null;
        }
    }
}

