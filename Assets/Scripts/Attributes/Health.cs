using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        LazyValue<int> healthPoints;
        LazyValue<int> maxHealthPoints;
        private bool isDead = false;

        public delegate void OnTakeDamage(AttackReport attackReport);
        public event OnTakeDamage onTakeDamage;

        Animator animator;
        BaseStats baseStats;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            baseStats = GetComponent<BaseStats>();
            healthPoints = new LazyValue<int>(GetInitialHealth);
            maxHealthPoints = new LazyValue<int>(GetInitialMaxHealth);
        }

        private int GetInitialHealth()
        {
            return maxHealthPoints.value;
        }

        private int GetInitialMaxHealth()
        {
            return baseStats.GetStat(Stat.Health);
        }

        private void Start()
        {
            healthPoints.ForceInit();
        }

        private void OnEnable()
        {
            baseStats.onLevelUp += LevelUpNewHealth;
        }

        private void OnDisable()
        {
            baseStats.onLevelUp -= LevelUpNewHealth;
        }

        private void LevelUpNewHealth()
        {
            float oldHealthPercentage = healthPoints.value / (float)maxHealthPoints.value;
            maxHealthPoints.value = baseStats.GetStat(Stat.Health);
            healthPoints.value = Mathf.RoundToInt(maxHealthPoints.value * oldHealthPercentage);
        }

        public bool TakeDamage(GameObject instigator, int damageTaken, int hitPrecision)
        {
            if (isDead) return false;
            if (!HasHit(instigator, hitPrecision))
            {
                onTakeDamage(new AttackReport(0, AttackResult.Miss));
                return false;
            }

            damageTaken = CalculateDamage(damageTaken);

            healthPoints.value = Mathf.Max(healthPoints.value - damageTaken, 0);
            onTakeDamage(new AttackReport(damageTaken, AttackResult.Hit));

            if (healthPoints.value < 1)
            {
                GrantExperience(instigator);
                Die();
            }

            return true;
        }

        private bool HasHit(GameObject instigator, int hitPrecision)
        {
            int swiftness = baseStats.GetStat(Stat.Swiftness);
            float hitChance = hitPrecision / (float)(hitPrecision + swiftness);

            if (Random.value < hitChance) return true;
            else return false;
        }

        private int CalculateDamage(int damageTaken)
        {
            int defense = baseStats.GetStat(Stat.Defense);
            damageTaken = Mathf.RoundToInt(Mathf.Pow(damageTaken, 2) / (damageTaken + defense));
            return damageTaken;
        }

        private void GrantExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetRewardXP());
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
            return healthPoints.value;
        }

        public int GetMaxHealthPoints()
        {
            return maxHealthPoints.value;
        }

        public object CaptureState()
        {
            return healthPoints.value;
        }

        public void RestoreState(object state)
        {
            healthPoints.value = (int)state;

            if (healthPoints.value < 1)
            {
                Die();
            }
        }
    }

    public class AttackReport
    {
        public int damageDealt = 0;
        public AttackResult result;

        public AttackReport()
        {
            result = AttackResult.None;
        }

        public AttackReport(int damageDealt, AttackResult result)
        {
            this.damageDealt = damageDealt;
            this.result = result;
        }
    }

    public enum AttackResult
    {
        None,
        Hit,
        Miss
    }
}
