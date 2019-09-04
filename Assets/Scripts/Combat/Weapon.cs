using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] UnityEvent onAttack;

        public void OnAttack()
        {
            onAttack.Invoke();
        }
    }
}


