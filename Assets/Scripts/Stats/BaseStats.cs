using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression;

        int currentLevel = 0;

        private void Start()
        {
            currentLevel = CalculateLevel();
        }

        private void Update()
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                Debug.Log("Level up!");
            }
        }

        public int GetStat(Stat stat)
        {
            return progression.GetStat(characterClass, stat, CalculateLevel());
        }

        public int GetLevel()
        {
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

