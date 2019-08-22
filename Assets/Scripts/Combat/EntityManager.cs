using UnityEngine;
using System.Collections.Generic;
using RPG.Stats;
using System;

namespace RPG.Combat
{
    public class EntityManager : MonoBehaviour
    {
        Dictionary<Alignment, List<CombatTarget>> entities;

        public event Action onUpdateEntities;

        private void Awake()
        {
            entities = new Dictionary<Alignment, List<CombatTarget>>();
        }

        public void RegisterEntity(CombatTarget entity, Alignment alignment)
        {

            if (!entities.ContainsKey(alignment))
            {
                entities[alignment] = new List<CombatTarget>();
            }

            if (entities[alignment].Contains(entity)) return;

            entities[alignment].Add(entity);

            onUpdateEntities();
        }

        public void ChangeAlignment(CombatTarget entity, Alignment oldAlignment, Alignment newAlignment)
        {
            entities[oldAlignment].Remove(entity);
            RegisterEntity(entity, newAlignment);
        }

        public List<CombatTarget> GetEnemies(CombatTarget entity, Alignment entityAlignment)
        {
            List<CombatTarget> allEnemies = new List<CombatTarget>();
            foreach(Alignment alignment in entities.Keys)
            {
                if (alignment == entityAlignment && 
                    entityAlignment != Alignment.Rogue) continue;

                allEnemies.AddRange(entities[alignment]);
            }

            if (allEnemies.Contains(entity))
            allEnemies.Remove(entity);

            return allEnemies;
        }
    }

    public enum Alignment
    {
        Lawful,
        Bandit,
        Rogue
    }
}

