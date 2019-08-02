using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression;

        public int GetHealth()
        {
            return progression.GetHealth(characterClass, startingLevel);
        }

        public int GetXP()
        {
            return 50;
        }
    }
}

