using RPG.Control;
using RPG.Attributes;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        Health health;
        BaseStats baseStats;
        const int criticalDamageMultiplier = 2;
        const float randomness = 0.2f;

        public delegate void OnAttacked(AttackReport attackReport);
        public event OnAttacked onAttacked;
        
        private void Awake()
        {
            health = GetComponent<Health>();
            baseStats = GetComponent<BaseStats>();
        }

        public bool HandleAttack(GameObject instigator, int damageTaken, int attackPrecision, int criticalStrike, bool isRanged)
        {
            if (GetIsDead()) return false;
            AttackReport attackReport = new AttackReport();
            
            if (!IsHit(instigator, attackPrecision))
            {
                attackReport.result = AttackResult.Miss;
                onAttacked(attackReport);
                return false;
            }

            damageTaken = CalculateDamage(damageTaken, criticalStrike, isRanged, out attackReport);
            health.TakeDamage(instigator, damageTaken);

            if (GetIsDead()) attackReport.result = AttackResult.TargetDown;
            onAttacked(attackReport);

            return true;
        }

        private bool IsHit(GameObject instigator, int hitPrecision)
        {
            int swiftness = baseStats.GetStat(Stat.Swiftness);
            float hitChance = hitPrecision / (float)(hitPrecision + swiftness);

            if (Random.value <= hitChance) return true;
            else return false;
        }

        private int CalculateDamage(int damageTaken, int criticalStrike, bool isRanged, out AttackReport attackReport)
        {
            attackReport = new AttackReport(AttackResult.Hit);

            int defense = baseStats.GetStat(Stat.Defense);
            damageTaken = Mathf.RoundToInt(Mathf.Pow(damageTaken, 2) / (damageTaken + defense));

            if (IsCriticalHit(criticalStrike, isRanged))
            {
                damageTaken *= criticalDamageMultiplier;
                attackReport.result = AttackResult.CriticalHit;
            }
            
            damageTaken = Mathf.RoundToInt(Random.Range(1 - randomness, 1 + randomness) * damageTaken);
            attackReport.damageDealt = damageTaken;

            return damageTaken;
        }

        private bool IsCriticalHit(int criticalStrike, bool isRanged)
        {
            Stat attackType = Stat.Strength;
            if (isRanged) attackType = Stat.Range;

            int critProtection = baseStats.GetBaseStat(attackType) + baseStats.GetStat(Stat.Defense);
            float critChance = criticalStrike / (float)(criticalStrike + critProtection);

            if (Random.value <= critChance) return true;
            else return false;
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (gameObject.tag == "Player") return false;

            Fighter playerFighter = callingController.GetComponent<Fighter>();
            if (!playerFighter.CanAttack(gameObject)) return false;

            if (Input.GetMouseButtonDown(0))
            {
                playerFighter.Attack(this);
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        public bool GetIsDead()
        {
            return health.GetIsDead();
        }
    }    
}
