using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPG.Combat
{
    // Organises entities into clans 
    public class EntityManager : MonoBehaviour
    {
        [SerializeField] CustomClan[] customClans;
        
        // Clan lookup
        Dictionary<Clan, CustomClan> clans = new Dictionary<Clan, CustomClan>();

        public event Action onUpdateEntities;

        private void Awake()
        {
            // Build clan lookup
            foreach(CustomClan clan in customClans)
            {
                clans[clan.name] = clan;
            }
        }

        // Entity added to clan
        public void RegisterEntity(CombatTarget entity, Clan clan)
        {
            CustomClan entityClan = clans[clan];
            entityClan.AddMember(entity);
            entity.SetClan(clan);

            onUpdateEntities?.Invoke();
        }

        // Entity removed from clan
        public void RemoveEntity(CombatTarget entity)
        {
            CustomClan entityClan = clans[entity.GetClan()];
            entityClan.RemoveMember(entity);
        }

        public void ChangeClan(CombatTarget entity, Clan newClan)
        {
            RemoveEntity(entity);
            RegisterEntity(entity, newClan);
        }


        // Gets list of enemies of hostile clans
        public List<CombatTarget> GetEnemies(CombatTarget entity)
        {
            List<CombatTarget> allEnemies = new List<CombatTarget>();

            CustomClan entityClan = clans[entity.GetClan()];

            foreach(CustomClan clan in clans.Values)
            {
                // Enemies if different allignment
                // Rogue clans always enemies
                if (clan.alignment != entityClan.alignment ||
                   (entityClan != clan && clan.alignment == Alignment.Rogue))
                {
                    allEnemies.AddRange(clan.GetMembers());
                }
            }

            return allEnemies;
        }  

        // Gets list of allies of friendly clans
        public List<CombatTarget> GetAllies(CombatTarget entity)
        {
            List<CombatTarget> allAllies = new List<CombatTarget>();

            CustomClan entityClan = clans[entity.GetClan()];

            foreach (CustomClan clan in clans.Values)
            {
                // Allies if same allignment
                // Rogue clans never allies
                if (clan.alignment == entityClan.alignment && clan.alignment != Alignment.Rogue ||
                    clan == entityClan)
                {
                    allAllies.AddRange(clan.GetMembers());
                }
            }

            if (allAllies.Contains(entity)) allAllies.Remove(entity);

            return allAllies;
        }

        // Checks if attack was friendly fire
        public void EvaluateAttack(CombatTarget aggressor, CombatTarget reciever)
        {
            CustomClan aggressorClan = clans[aggressor.GetClan()];
            CustomClan recieverClan = clans[reciever.GetClan()];

            if (aggressorClan.alignment != recieverClan.alignment || 
                aggressorClan.alignment == Alignment.Rogue) return;

            // Entity that attacks ally becomes rogue 
            aggressorClan.alignment = Alignment.Rogue;
            onUpdateEntities();
        }
    }

    public enum Alignment
    {
        Lawful,
        Rebel,
        Rogue
    }

    public enum Clan
    {
        TheCircleOfOssus,
        KnightsOfEverlance,
        PlayerParty,
        Bandit,
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

