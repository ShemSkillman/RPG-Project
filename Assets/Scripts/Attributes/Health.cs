using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;
using System;
using System.Collections;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        LazyValue<int> healthPoints;
        LazyValue<int> maxHealthPoints;
        bool isDead = false;
        
        public UnityEvent onTakeDamage, onDie;
        public event Action onHealthChange;

        [Header("Body Sink Configuration:")]
        [SerializeField] float minSinkSpeed = 0.0001f;
        [SerializeField] float maxSinkSpeed = 0.001f;
        [SerializeField] float maxSinkDistance = 1f;

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

            onTakeDamage.Invoke();
            onHealthChange();

            if (healthPoints.value < 1)
            {
                onDie.Invoke();
                GrantExperience(instigator);
                Die();
            }

            return true;
        }

        public void Heal(int healthToRestore)
        {
            if (isDead) return;

            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, maxHealthPoints.value);
            onHealthChange();
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
            StartCoroutine(BodySink());
        }

        private IEnumerator BodySink()
        {
            float sinkDistance = 0f;
            float sinkSpeed = UnityEngine.Random.Range(minSinkSpeed, maxSinkSpeed);
            while (sinkDistance < maxSinkDistance)
            {
                sinkDistance += Time.deltaTime * sinkSpeed;
                transform.Translate(Vector3.down * Time.deltaTime * sinkSpeed);
                
                yield return null;
            }

            Destroy(gameObject);
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
