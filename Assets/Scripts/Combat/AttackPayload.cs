using System;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    public class AttackPayload
    {
        public GameObject instigator;
        public Stat attackType;
        public int attackPoints;
        public int critStrike;
        public int damage;
        public int hitPrecision;

        public AttackPayload(BaseStats baseStats, Weapon weapon)
        {
            instigator = baseStats.gameObject;
            attackType = GetAttackType(weapon);
            attackPoints = baseStats.GetStat(attackType);
            critStrike = GetCriticalStrike(baseStats, weapon);
            damage = GetDamage(baseStats, weapon);
            hitPrecision = GetHitPrecision(baseStats, weapon);
        }

        private Stat GetAttackType(Weapon weapon)
        {
            Stat attackType = Stat.Strength;
            if (weapon.HasProjectile()) attackType = Stat.Range;
            return attackType;
        }

        private int GetCriticalStrike(BaseStats baseStats, Weapon weapon)
        {
            return baseStats.GetStat(attackType) * 2;
        }

        public int GetHitPrecision(BaseStats baseStats, Weapon weapon)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Swiftness);
        }

        private int GetDamage(BaseStats baseStats, Weapon weapon)
        {
            return Mathf.RoundToInt(baseStats.GetStat(attackType) * weapon.GetWeaponWeight());
        }

    }
}

