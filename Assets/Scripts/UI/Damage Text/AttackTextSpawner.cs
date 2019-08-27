using UnityEngine;
using RPG.Combat;
using RPG.Attributes;

namespace RPG.UI.DamageText
{
    public class AttackTextSpawner : MonoBehaviour
    {
        [SerializeField] AttackText damageText;

        [SerializeField] float textMaxIdleTime = 1f;
        float textIdleTime = 0f;
        int currentMissCount = 0;
        int currentDamageCount = 0;
        AttackText currentInstance;

        CombatTarget combatTarget;
        Health health;

        private void Awake()
        {
            combatTarget = GetComponentInParent<CombatTarget>();
            health = GetComponentInParent<Health>();
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
            if (currentInstance != null)
            {
                textIdleTime = 0f;
                SetCurrentText(attackReport);
                return;
            }

            currentInstance = Instantiate(damageText, transform);

            switch (attackReport.result)
            {
                case AttackResult.Miss:
                    currentInstance.SetText("Dodge!");
                    currentMissCount++;
                    break;
                case AttackResult.CriticalHit:
                    currentDamageCount += attackReport.damageDealt;
                    currentInstance.SetText(currentDamageCount.ToString());
                    break;
                case AttackResult.TargetDown:
                    currentInstance.SetText("Dead!");
                    break;
                case AttackResult.Hit:
                    currentDamageCount += attackReport.damageDealt;
                    currentInstance.SetText(currentDamageCount.ToString());
                    break;
            }
        }

        private void SetCurrentText(AttackReport attackReport)
        {
            if (attackReport.result == AttackResult.Hit || attackReport.result == AttackResult.CriticalHit)
            {
                if (currentDamageCount < 1)
                {
                    TextFadeOut();
                    Spawn(attackReport);
                }
                else
                {
                    currentDamageCount += attackReport.damageDealt;
                    currentInstance.SetText(currentDamageCount.ToString());
                }
            }
            else if (attackReport.result == AttackResult.TargetDown)
            {
                TextFadeOut();
                Spawn(attackReport);
            }
            else if (attackReport.result == AttackResult.Miss)
            {
                if (currentDamageCount > 0) return;
                currentMissCount++;
                currentInstance.SetText("x" + currentMissCount.ToString() + " Dodge!");
            }

        }

        private void TextFadeOut()
        {
            currentInstance.GetComponent<Animator>().SetTrigger("fadeOut");
            textIdleTime = 0f;
            currentMissCount = 0;
            currentDamageCount = 0;
            currentInstance = null;
        }
    }
}

