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

        public AttackPayload(BaseStats baseStats, WeaponConfig weapon)
        {
            instigator = baseStats.gameObject;
            attackType = GetAttackType(weapon);
            attackPoints = baseStats.GetStat(attackType);
            critStrike = GetCriticalStrike(baseStats, weapon);
            damage = GetDamage(baseStats, weapon);
            hitPrecision = GetHitPrecision(baseStats, weapon);
        }

        private Stat GetAttackType(WeaponConfig weapon)
        {
            Stat attackType = Stat.Strength;
            if (weapon.HasProjectile()) attackType = Stat.Range;
            return attackType;
        }

        private int GetCriticalStrike(BaseStats baseStats, WeaponConfig weapon)
        {
            return baseStats.GetStat(attackType) * 2;
        }

        public int GetHitPrecision(BaseStats baseStats, WeaponConfig weapon)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Swiftness);
        }

        private int GetDamage(BaseStats baseStats, WeaponConfig weapon)
        {
            return Mathf.RoundToInt(baseStats.GetStat(attackType) * weapon.GetWeaponWeight());
        }

    }
}

