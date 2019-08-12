using UnityEngine;
using TMPro;

namespace RPG.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageText;

        public void Spawn(int damageAmount)
        {
            DamageText instance = Instantiate(damageText, transform);
            instance.SetValue(damageAmount);
        }
    }
}

