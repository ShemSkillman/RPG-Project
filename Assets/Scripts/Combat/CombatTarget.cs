using RPG.Core;
using RPG.Control;
using RPG.Resources;
using UnityEngine;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        Fighter fighter;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            Fighter playerFighter = callingController.GetComponent<Fighter>();
            if (!playerFighter.CanAttack(gameObject)) return false;

            if (Input.GetMouseButtonDown(0))
            {
                playerFighter.Attack(gameObject);
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }
    }
}
