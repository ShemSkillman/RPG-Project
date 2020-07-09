using UnityEngine;
using UnityEngine.UI;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;
        BaseStats baseStats;
        Text experienceText;

        int nextLevelXP;
        int usedXP;

        private void Awake()
        {
            // Only player XP displayed
            GameObject player = GameObject.FindWithTag("Player");
            experience = player.GetComponent<Experience>();
            baseStats = player.GetComponent<BaseStats>();
            experienceText = GetComponent<Text>(); 
        }

        private void Start()
        {
            CalculateXPWindow();
        }

        private void OnEnable()
        {
            experience.onExperienceGained += UpdateDisplay;
            baseStats.onLevelUp += CalculateXPWindow;
        }

        private void OnDisable()
        {
            experience.onExperienceGained -= UpdateDisplay;
            baseStats.onLevelUp -= CalculateXPWindow;
        }

        // XP window is the amount of XP gained above the current level
        // XP gained must exceed threshold (required XP) to level up
        private void CalculateXPWindow()
        {
            nextLevelXP = baseStats.CalculateXPRequiredForLevel(baseStats.GetLevel() + 1);
            usedXP = baseStats.CalculateXPRequiredForLevel(baseStats.GetLevel());

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Used XP = XP already used to get to current level
            int currentXP = experience.GetTotalXP() - usedXP;
            int targetXP = nextLevelXP - usedXP;

            // Character reached max level
            if (nextLevelXP < 1)
            {
                experienceText.text = string.Format("XP: {0}", currentXP);
            }
            // Displays current / target
            else
            {
                experienceText.text = string.Format("XP: {0}/{1}", currentXP, targetXP);
            }
        }
    }
}

