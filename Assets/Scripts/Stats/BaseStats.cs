using System;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression;
        [SerializeField] GameObject levelUpParticleEffect;

        int currentLevel = 0;
        public event Action onLevelUp;

        private void Start()
        {
            currentLevel = CalculateLevel();

            Experience experience = GetComponent<Experience>();
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
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
            return (GetBaseStat(stat) + GetAdditiveMultiplier(stat)) * (1 + GetPercentageModifier(stat)/100);
        }

        private int GetBaseStat(Stat stat)
        {
            return progression.GetStat(characterClass, stat, CalculateLevel());
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
            if (currentLevel < 1)
            {
                currentLevel = CalculateLevel();
            }
            return currentLevel;
        }

        public int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return startingLevel;

            int currentXP = experience.GetExperience();

            int[] XPLevels = progression.GetStatLevels(characterClass, Stat.ExperienceToLevelUp);
            int level = 1;

            for (int i = 0; i < XPLevels.Length; i++, level++)
            {
                int levelXP = XPLevels[i];
                if (currentXP < levelXP) break;
            }

            return level;
        }
    }
}

