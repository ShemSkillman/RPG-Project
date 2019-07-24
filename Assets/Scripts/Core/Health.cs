using UnityEngine;
using RPG.Saving;

namespace RPG.Core
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] int healthPoints = 100;
        private bool isDead = false;

        Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void TakeDamage(int damageTaken)
        {
            if (isDead) return;         

            healthPoints = Mathf.Max(healthPoints - damageTaken, 0);

            if (healthPoints < 1)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            animator.SetTrigger("death");
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        public bool GetIsDead()
        {
            return isDead;
        }

        public object CaptureState()
        {
            return healthPoints;
        }

        public void RestoreState(object state)
        {
            healthPoints = (int)state;

            if (healthPoints < 1)
            {
                Die();
            }
        }
    }
}
