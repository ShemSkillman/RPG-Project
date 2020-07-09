using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        // Events not used!
        [SerializeField] UnityEvent onStartAttack;
        [SerializeField] UnityEvent onEndAttack;

        public void OnStartAttack()
        {
            onStartAttack.Invoke();
        }

        public void OnEndAttack()
        {
            onEndAttack.Invoke();
        }
    }
}


