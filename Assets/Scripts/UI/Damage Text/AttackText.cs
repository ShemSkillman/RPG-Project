using UnityEngine;
using TMPro;

namespace RPG.UI.DamageText
{
    public class AttackText : MonoBehaviour
    {
        TextMeshProUGUI textMeshPro;

        private void Awake()
        {
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void DestroyText()
        {
            Destroy(gameObject);
        }

        public void SetText(string message)
        {
            textMeshPro.text = message;
        }

        public void SetTextColor(Color color)
        {
            textMeshPro.color = color;
        }
    }
}

