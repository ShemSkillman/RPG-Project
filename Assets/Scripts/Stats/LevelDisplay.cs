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

        private void Update()
        {
            levelText.text = string.Format("Level: {0}", baseStats.CalculateLevel());
        }
    }
}
