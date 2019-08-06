using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        int healthPoints = -1;
        int maxHealthPoints;
        private bool isDead = false;

        Animator animator;
        BaseStats baseStats;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            baseStats = GetComponent<BaseStats>();

            baseStats.onLevelUp += LevelUpNewHealth;
            maxHealthPoints = baseStats.GetStat(Stat.Health);
            if (healthPoints < 0)
            {
                healthPoints = maxHealthPoints;
            }
        }

        private void LevelUpNewHealth()
        {
            float oldHealthPercentage = healthPoints / (float)maxHealthPoints;
            maxHealthPoints = baseStats.GetStat(Stat.Health);
            healthPoints = Mathf.RoundToInt(maxHealthPoints * oldHealthPercentage);
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

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
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

        public int GetHealthPoints()
        {
            return healthPoints;
        }

        public int GetMaxHealthPoints()
        {
            return maxHealthPoints;
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
