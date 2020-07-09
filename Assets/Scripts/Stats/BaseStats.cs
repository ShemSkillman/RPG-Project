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

        // State
        Experience experience;
        LazyValue<int> currentLevel;
        int nextLevelXP;

        // Events
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

        // Called once at start
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

        // Called when XP gained
        // Checks total XP > required XP to level up
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

        // Gets character stat with added bonuses
        public int GetStat(Stat stat)
        {
            return Mathf.RoundToInt((GetBaseStat(stat) + GetAdditiveMultiplier(stat)) * GetPercentageModifier(stat));
        }

        // XP given to enemy when this character dies
        public int GetRewardXP()
        {
            return progression.GetRewardXP(currentLevel.value);
        }

        // Character stat varies depeding on character class and current level
        private int GetBaseStat(Stat stat)
        {
            return progression.GetStat(characterClass, stat, GetLevel());
        }

        // Added bonus to character stat
        private int GetAdditiveMultiplier(Stat stat)
        {
            int total = 0;

            // Currently only fighter modifies stat values
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                // Adds all potential bonuses in provider
                foreach(int modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }

            return total;
        }

        // Multiplies (base + additive) to give resultant stat
        private float GetPercentageModifier(Stat stat)
        {
            int total = 0;

            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (int modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }

            return 1 + (total / 100f);
        }

        public int GetLevel()
        {
            return currentLevel.value;
        }

        // Recursive function 
        // Increments through levels until total XP is lower than XP for next level
        private int CalculateLevel(int level)
        {
            int totalXP = experience.GetTotalXP();

            // nextLevelXP is -1 when character reaches highest level
            if (nextLevelXP < 0 || 
                totalXP < nextLevelXP) 
                return level;

            level++;
            nextLevelXP = CalculateXPRequiredForLevel(level + 1);

            return CalculateLevel(level);
        }

        // Calculates XP required for given level
        public int CalculateXPRequiredForLevel(int level)
        {
            // All characters start at level 1
            if (level < 2) return 0;

            int[] XPLevels = progression.GetXPLevels();

            // XP table starts at level 2
            if (level - 2 < XPLevels.Length)
            {
                return XPLevels[level - 2];
            }

            return -1;
        }

    }
}

