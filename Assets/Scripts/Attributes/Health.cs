using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;
using System;
using System.Collections;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        LazyValue<int> healthPoints;
        LazyValue<int> maxHealthPoints;
        private bool isDead = false;
        
        [SerializeField] float sinkSpeed = 1f;
        
        public Action onHealthChange;

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

        public bool TakeDamage(GameObject instigator, int damageTaken)
        {
            if (isDead) return false;

            healthPoints.value = Mathf.Max(healthPoints.value - damageTaken, 0);
            onHealthChange();

            if (healthPoints.value < 1)
            {
                GrantExperience(instigator);
                Die();
            }

            return true;
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
            StartCoroutine(BodySink());
        }

        private IEnumerator BodySink()
        {
            float sinkDistance = 0f;
            float maxSinkDistance = UnityEngine.Random.value;
            while (sinkDistance < maxSinkDistance)
            {
                sinkDistance += Time.deltaTime * sinkSpeed;
                transform.Translate(Vector3.down * Time.deltaTime * sinkSpeed);
                
                yield return null;
            }
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
}
