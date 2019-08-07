using UnityEngine;
using UnityEngine.UI;

namespace RPG.Stats
{
    public class LevelDisplay : MonoBehaviour
    {
        BaseStats baseStats;
        Text levelText;

        private void Awake()
        {
            baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
            levelText = GetComponent<Text>();
        }

        private void Start()
        {
            UpdateDisplay();
        }

        private void OnEnable()
        {
            baseStats.onLevelUp += UpdateDisplay;
        }

        private void OnDisable()
        {
            baseStats.onLevelUp -= UpdateDisplay;
        }

        private void UpdateDisplay()
        {
            levelText.text = string.Format("Level: {0}", baseStats.GetLevel());
        }
    }
}
