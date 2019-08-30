using UnityEngine;
using RPG.Combat;
using TMPro;

namespace RPG.UI.DamageText
{
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

        private void SetText()
        {
            string message = "";

            if (isDead)
            {
                message = "Dead!";
            }
            else if (damageCount > 0)
            {
                message += damageCount.ToString();
            }
            else if (missCount > 0)
            {
                if (missCount == 1) message = "Dodge!";
                else message += "Dodge x" + missCount.ToString();
            }

            textMeshPro.text = message;
        }
    }
}

