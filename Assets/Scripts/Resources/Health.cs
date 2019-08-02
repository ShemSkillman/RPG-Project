using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] int healthPoints = 100;
        private bool isDead = false;

        Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            healthPoints = GetComponent<BaseStats>().GetHealth();
        }

        public void TakeDamage(GameObject instigator, int damageTaken)
        {
            if (isDead) return;         

            healthPoints = Mathf.Max(healthPoints - damageTaken, 0);

            if (healthPoints < 1)
            {
                GrantExperience(instigator);
                Die();
            }
        }

        private void GrantExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetXP());
        }

        public float GetPercentage()
        {
            return 100 * healthPoints / GetComponent<BaseStats>().GetHealth();
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
