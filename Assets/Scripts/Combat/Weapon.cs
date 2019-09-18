using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
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


