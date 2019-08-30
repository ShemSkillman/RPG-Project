using UnityEngine;
using RPG.Combat;
using RPG.Attributes;

namespace RPG.UI.DamageText
{
    public class AttackTextSpawner : MonoBehaviour
    {
        [SerializeField] AttackText damageText;
        [SerializeField] float textMaxIdleTime = 1f;

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
            if (currentInstance != null)
            {
                textIdleTime += Time.deltaTime;

                if (textIdleTime > textMaxIdleTime)
                    TextFadeOut();
            }
        }

        public void Spawn(AttackReport attackReport)
        {
            textIdleTime = 0f;

            if (currentInstance != null)
            {
                currentInstance.ExtractReport(attackReport);
                return;
            }

            currentInstance = Instantiate(damageText, transform);
            currentInstance.ExtractReport(attackReport);
        }

        private void TextFadeOut()
        {
            currentInstance.GetComponent<Animator>().SetTrigger("fadeOut"); ;
            currentInstance = null;
        }
    }
}

