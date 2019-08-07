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

        private void CalculateXPWindow()
        {
            nextLevelXP = baseStats.CalculateXPRequiredForLevel(baseStats.GetLevel() + 1);
            usedXP = baseStats.CalculateXPRequiredForLevel(baseStats.GetLevel());

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            int currentXP = experience.GetTotalXP() - usedXP;
            int targetXP = nextLevelXP - usedXP;

            if (nextLevelXP < 1)
            {
                experienceText.text = string.Format("XP: {0}", currentXP);
            }
            else
            {
                experienceText.text = string.Format("XP: {0}/{1}", currentXP, targetXP);
            }
        }
    }
}

