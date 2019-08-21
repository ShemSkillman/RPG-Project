using UnityEngine;
using TMPro;

namespace RPG.UI.DamageText
{
    public class AttackText : MonoBehaviour
    {
        public void DestroyText()
        {
            Destroy(gameObject);
        }

        public void SetText(string message, Color color)
        {
            TextMeshProUGUI textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            textMeshPro.color = color;
            textMeshPro.text = message;
        }
    }
}

