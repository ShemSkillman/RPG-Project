using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPG.Combat
{
    public class EntityManager : MonoBehaviour
    {
        [SerializeField] CustomClan[] customClans;
        
        Dictionary<Clan, CustomClan> clans = new Dictionary<Clan, CustomClan>();

        public event Action onUpdateEntities;

        private void Awake()
        {
            foreach(CustomClan clan in customClans)
            {
                clans[clan.name] = clan;
            }
        }

        public void RegisterEntity(CombatTarget entity, Clan clan)
        {
            CustomClan entityClan = clans[clan];
            entityClan.AddMember(entity);
            entity.SetClan(clan);

            onUpdateEntities();
        }

        public void RemoveEntity(CombatTarget entity)
        {
            CustomClan entityClan = clans[entity.GetClan()];
            entityClan.RemoveMember(entity);
            entity.SetClan(Clan.None);
        }

        public void ChangeClan(CombatTarget entity, Clan newClan)
        {
            RemoveEntity(entity);
            RegisterEntity(entity, newClan);
        }

        public List<CombatTarget> GetEnemies(CombatTarget entity)
        {
            List<CombatTarget> allEnemies = new List<CombatTarget>();

            CustomClan entityClan = clans[entity.GetClan()];

            foreach(CustomClan clan in clans.Values)
            {
                if (clan.alignment != entityClan.alignment ||
                   (entityClan != clan && clan.alignment == Alignment.Rogue))
                {
                    allEnemies.AddRange(clan.GetMembers());
                }
            }

            return allEnemies;
        }  

        public void EvaluateAttack(CombatTarget aggressor, CombatTarget reciever)
        {
            CustomClan aggressorClan = clans[aggressor.GetClan()];
            CustomClan recieverClan = clans[reciever.GetClan()];

            if (aggressorClan.alignment != recieverClan.alignment || 
                aggressorClan.alignment == Alignment.Rogue) return;

            aggressorClan.alignment = Alignment.Rogue;
            onUpdateEntities();
        }
    }

    public enum Alignment
    {
        Lawful,
        Bandit,
        Rogue
    }

    public enum Clan
    {
        TheCircleOfOssus,
        KnightsOfEverlance,
        PlayerParty,
        None
    }

    [System.Serializable]
    public class CustomClan
    {
        public Clan name;
        public Alignment alignment;
        List<CombatTarget> clanMembers = new List<CombatTarget>();

        public void AddMember(CombatTarget member)
        {
            if (clanMembers.Contains(member)) return;
            clanMembers.Add(member);
        }

        public void RemoveMember(CombatTarget member)
        {
            if (clanMembers.Contains(member))
            {
                clanMembers.Remove(member);
            }
        }

        public List<CombatTarget> GetMembers()
        {
            return clanMembers;
        }
    }
}

