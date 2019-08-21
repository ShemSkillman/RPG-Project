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

        public void SetText(string message)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = message;
        }
    }
}

