using System;
using UnityEngine;
using GameDevTV.Utils;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression;
        [SerializeField] GameObject levelUpParticleEffect;

        Experience experience;
        LazyValue<int> currentLevel;
        int nextLevelXP;

        public event Action onLevelUp;

        private void Awake()
        {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(GetInitialCurrentLevel);
        }

        private void Start()
        {
            currentLevel.ForceInit();
        }

        private int GetInitialCurrentLevel()
        {
            if (experience == null) return startingLevel;

            nextLevelXP = CalculateXPRequiredForLevel(startingLevel + 1);
            return CalculateLevel(startingLevel);
        }

        private void OnEnable()
        {
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel()
        {
            int newLevel = CalculateLevel(currentLevel.value);
            if (newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                LevelUpEffect();
                onLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(levelUpParticleEffect, transform);
        }

        public int GetStat(Stat stat)
        {
            return Mathf.RoundToInt((GetBaseStat(stat) + GetAdditiveMultiplier(stat)) * (1 + GetPercentageModifier(stat)/100f));
        }

        private int GetBaseStat(Stat stat)
        {
            return progression.GetStat(characterClass, stat, GetLevel());
        }

        private int GetAdditiveMultiplier(Stat stat)
        {
            int total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach(int modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }

            return total;
        }

        private int GetPercentageModifier(Stat stat)
        {
            int total = 0;
            foreach(IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach(int modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }

            return total;
        }

        public int GetLevel()
        {
            return currentLevel.value;
        }

        private int CalculateLevel(int level)
        {
            int totalXP = experience.GetTotalXP();
            if (nextLevelXP < 0 || totalXP < nextLevelXP) return level;

            level++;
            nextLevelXP = CalculateXPRequiredForLevel(level + 1);

            return CalculateLevel(level);
        }

        public int CalculateXPRequiredForLevel(int level)
        {
            if (level < 2) return 0;

            int[] XPLevels = progression.GetStatLevels(characterClass, Stat.ExperienceToLevelUp);
            if (level - 2 < XPLevels.Length)
            {
                return XPLevels[level - 2];
            }

            return -1;
        }

    }
}

