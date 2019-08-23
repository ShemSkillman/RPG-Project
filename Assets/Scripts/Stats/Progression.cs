using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression")]
    public class Progression : ScriptableObject
    {
        [Header("Character Customization")]
        [SerializeField] Character[] characters;

        [Header("Other configs")]
        [SerializeField] float healthMultiplier = 1f;

        [Header("Stock Stat tables")]
        [SerializeField] public int[] statTable;
        [SerializeField] int[] xpRewardTable;
        [SerializeField] int[] levelXPTable;

        Dictionary<CharacterClass, Dictionary<Stat, float>> charStatMultiplierLookup;

        public int GetStat(CharacterClass characterClass, Stat stat, int level)
        {
            BuildLookUp();

            int stockStat = GetStockStat(stat, level);
            float multiplier = 1f;

            if (charStatMultiplierLookup[characterClass].ContainsKey(stat))
            {
                multiplier = charStatMultiplierLookup[characterClass][stat];
            }

            return Mathf.RoundToInt(stockStat * multiplier);
        }

        public int GetRewardXP(int level)
        {
            return xpRewardTable[level - 1];
        }

        public int[] GetXPLevels()
        {
            return levelXPTable;
        }

        private int GetStockStat(Stat stat, int level)
        {
            int stockStat;
            if (level - 1 > statTable.Length) stockStat = statTable[statTable.Length - 1];
            else stockStat = statTable[level - 1];

            if (stat == Stat.Health)
            {
                stockStat = Mathf.RoundToInt(healthMultiplier * stockStat);
            }

            return stockStat;
        } 

        private void BuildLookUp()
        {
            if (charStatMultiplierLookup != null) return;

            charStatMultiplierLookup = new Dictionary<CharacterClass, Dictionary<Stat, float>>();

            foreach (Character character in characters)
            {
                var statMultiplierDict = new Dictionary<Stat, float>();
                foreach (StatModifier statMod in character.statModifiers)
                {
                    statMultiplierDict[statMod.stat] = statMod.statMultiplier;
                }

                charStatMultiplierLookup[character.characterClass] = statMultiplierDict;
            }
        }

        [System.Serializable]
        class Character
        {
            public CharacterClass characterClass;
            public StatModifier[] statModifiers;
        }

        [System.Serializable]
        class StatModifier
        {
            public Stat stat;
            public float statMultiplier;
        }
    }
}
