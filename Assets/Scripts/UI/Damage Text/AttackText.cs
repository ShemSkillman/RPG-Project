using UnityEngine;
using RPG.Combat;
using TMPro;

namespace RPG.UI.DamageText
{
    // Stores all results of an attack move
    public class AttackText : MonoBehaviour
    {
        TextMeshProUGUI textMeshPro;
        
        int missCount = 0;
        int damageCount = 0;
        bool isDead = false;

        private void Awake()
        {
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void DestroyText()
        {
            Destroy(gameObject);
        }

        // Evaluate attack results
        public void ExtractReport(AttackReport attackReport)
        {

            switch (attackReport.result)
            {
                case AttackResult.Miss:
                    missCount++;
                    break;
                case AttackResult.TargetDown:
                    isDead = true;
                    break;
                case AttackResult.Hit:
                    damageCount += attackReport.damageDealt;
                    break;
                case AttackResult.CriticalHit:
                    damageCount += attackReport.damageDealt;
                    break;

            }
            
            SetText();
        }

        // Displays highest priority info to user
        private void SetText()
        {
            string message = "";

            // Death
            if (isDead)
            {
                message = "Dead!";
            }
            // Damage
            else if (damageCount > 0)
            {
                message += damageCount.ToString();
            }
            // Dodge
            else if (missCount > 0)
            {
                if (missCount == 1) message = "Dodge!";
                else message += "Dodge x" + missCount.ToString();
            }

            textMeshPro.text = message;
        }
    }
}

