using System;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    // Used to evaluate attack on target
    public class AttackPayload
    {
        public GameObject instigator;
        public Stat attackType;
        public int attackPoints;
        public int critStrike;
        public int damage;
        public int hitPrecision;

        public AttackPayload(BaseStats baseStats, WeaponConfig weapon)
        {
            instigator = baseStats.gameObject;
            attackType = GetAttackType(weapon);
            attackPoints = baseStats.GetStat(attackType);
            critStrike = GetCriticalStrike(baseStats, weapon);
            damage = GetDamage(baseStats, weapon);
            hitPrecision = GetHitPrecision(baseStats, weapon);
        }

        // Melee or range?
        private Stat GetAttackType(WeaponConfig weapon)
        {
            // Strength == melee
            Stat attackType = Stat.Strength;
            if (weapon.HasProjectile()) attackType = Stat.Range;
            return attackType;
        }

        // Critical = (attack type stat) x2
        private int GetCriticalStrike(BaseStats baseStats, WeaponConfig weapon)
        {
            return baseStats.GetStat(attackType) * 2;
        }

        // Precision = (attack type stat) + swiftness
        public int GetHitPrecision(BaseStats baseStats, WeaponConfig weapon)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Swiftness);
        }

        // Total damage = (attack type stat) * weapon weight
        private int GetDamage(BaseStats baseStats, WeaponConfig weapon)
        {
            return Mathf.RoundToInt(baseStats.GetStat(attackType) * weapon.GetWeaponWeight());
        }

    }
}

