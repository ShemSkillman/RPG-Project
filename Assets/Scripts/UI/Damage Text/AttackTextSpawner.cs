using UnityEngine;
using RPG.Combat;
using TMPro;

namespace RPG.UI.DamageText
{
    public class AttackTextSpawner : MonoBehaviour
    {
        [SerializeField] AttackText damageText;
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

        public void Spawn(AttackReport attackReport)
        {
            AttackText instance = Instantiate(damageText, transform);

            switch (attackReport.result)
            {
                case AttackResult.Miss:
                    instance.SetText("Dodge!", Color.white);
                    break;
                case AttackResult.CriticalHit:
                    instance.SetText(attackReport.damageDealt.ToString(), Color.red);
                    break;
                case AttackResult.TargetDown:
                    instance.SetText("Dead!", Color.white);
                    break;
                case AttackResult.Hit:
                    instance.SetText(attackReport.damageDealt.ToString(), Color.white);
                    break;
            }
        }
    }
}

