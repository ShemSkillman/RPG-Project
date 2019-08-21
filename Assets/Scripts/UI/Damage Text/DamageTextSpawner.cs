using UnityEngine;
using RPG.Attributes;
using TMPro;

namespace RPG.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageText;
        Health health;

        private void Awake()
        {
            health = GetComponentInParent<Health>();
        }

        private void OnEnable()
        {
            health.onTakeDamage += Spawn;
        }

        private void OnDisable()
        {
            health.onTakeDamage -= Spawn;
        }

        public void Spawn(AttackReport attackReport)
        {
            DamageText instance = Instantiate(damageText, transform);

            if (attackReport.result == AttackResult.Miss) instance.SetText("Miss!");
            else instance.SetText(attackReport.damageDealt.ToString());
        }
    }
}

