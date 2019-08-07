using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Resources;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        // Cache references
        private Mover mover;
        private Fighter fighter;
        private Health health;

        void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            if (health.GetIsDead()) return;
            if (InteractWithCombat()) return;
            if (InteractWithMovement()) return;
            Debug.Log("Can do nothing here...");
        }

        private bool InteractWithCombat()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if (target == null) continue;
                
                if (target != null && 
                    fighter.CanAttack(target.gameObject))
                {
                    if (Input.GetMouseButton(0))
                    {
                        fighter.Attack(target.gameObject);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool InteractWithMovement()
        {
            bool hasHit = Physics.Raycast(GetMouseRay(), out RaycastHit hit);
            if (hasHit)
            {
                if (Input.GetMouseButton(0))
                {
                    mover.StartMoveAction(hit.point, 1f);
                }
                return true;
            }
            return false;
        }

        private Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
