using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    public class AttackPayload
    {
        public GameObject instigator;
        public Stat attackType;
        public int attackPoints;
        public int damage;
        public int hitPrecision;

        public AttackPayload(BaseStats baseStats, Stat attackType, Weapon weapon)
        {
            instigator = baseStats.gameObject;
            this.attackType = attackType;
            attackPoints = baseStats.GetStat(attackType);
            damage = GetDamage(baseStats, attackType, weapon);
            hitPrecision = GetHitPrecision(baseStats, attackType);
        }

        private int GetDamage(BaseStats baseStats, Stat attackType, Weapon weapon)
        {
            return Mathf.RoundToInt(baseStats.GetStat(attackType) * weapon.GetWeaponWeight());
        }

        public int GetHitPrecision(BaseStats baseStats, Stat attackType)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Swiftness);
        }
    }
}

