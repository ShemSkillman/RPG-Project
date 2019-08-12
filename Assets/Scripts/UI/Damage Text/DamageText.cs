using UnityEngine;
using TMPro;

namespace RPG.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        public void DestroyText()
        {
            Destroy(gameObject);
        }

        public void SetValue(int damageAmount)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = damageAmount.ToString();
        }
    }
}

