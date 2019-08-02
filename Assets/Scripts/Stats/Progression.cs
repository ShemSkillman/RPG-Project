using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression")]
    public class Progression : ScriptableObject
    {
        [SerializeField] ProgressionCharacterClass[] characterClasses;

        Dictionary<CharacterClass, Dictionary<Stat, int[]>> lookupTable;

        public int GetStat(CharacterClass characterClass, Stat stat, int level)
        {
            BuildLookUp();

            int[] levels =  lookupTable[characterClass][stat];

            if (level > levels.Length)
            {
                Debug.Log(stat + " value could not be found for " + characterClass + " of level " + level);
                return levels[levels.Length - 1];
            }

            return levels[level - 1];
        }

        public int[] GetStatLevels(CharacterClass characterClass, Stat stat)
        {
            BuildLookUp();

            return lookupTable[characterClass][stat];
        }

        private void BuildLookUp()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, int[]>>();

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, int[]>();

                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                lookupTable[progressionClass.characterClass] = statLookupTable;
            }
        }

        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats;
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public int[] levels;
        }
    }
}
