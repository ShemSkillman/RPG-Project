using RPG.Attributes;
using RPG.Stats;
using UnityEngine;
using RPG.Control.Cursor;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        Health health;
        BaseStats baseStats;
        EntityManager entityManager;
        const int criticalDamageMultiplier = 2;
        [SerializeField] Alignment alignment = Alignment.Lawful;

        public delegate void OnAttacked(AttackReport attackReport);
        public event OnAttacked onAttacked;
        
        private void Awake()
        {
            health = GetComponent<Health>();
            baseStats = GetComponent<BaseStats>();
            entityManager = FindObjectOfType<EntityManager>();
        }

        private void Start()
        {
            entityManager.RegisterEntity(this, alignment);
        }

        public bool HandleAttack(AttackPayload attackPayload)
        {
            if (GetIsDead()) return false;
            AttackReport attackReport = new AttackReport();
            
            if (!IsHit(attackPayload.hitPrecision))
            {
                attackReport.result = AttackResult.Miss;
                onAttacked(attackReport);
                return false;
            }

            int damage = CalculateDamage(attackPayload.damage, attackPayload.attackPoints, attackPayload.attackType, out attackReport);
            health.TakeDamage(attackPayload.instigator, damage);

            if (GetIsDead()) attackReport.result = AttackResult.TargetDown;
            onAttacked(attackReport);

            return true;
        }

        private bool IsHit(int hitPrecision)
        {
            float hitChance = hitPrecision / (float)(hitPrecision + baseStats.GetStat(Stat.Swiftness));

            if (Random.value <= hitChance) return true;
            else return false;
        }

        private int CalculateDamage(int damage, int attackPoints, Stat attackType, out AttackReport attackReport)
        {
            attackReport = new AttackReport(AttackResult.Hit);
            
            float defenseMult = attackPoints / (float)(baseStats.GetStat(Stat.Defense) + attackPoints);
            damage = Mathf.RoundToInt(damage * defenseMult);

            if (IsCriticalHit(attackPoints, attackType))
            {
                damage *= criticalDamageMultiplier;
                attackReport.result = AttackResult.CriticalHit;
            }

            attackReport.damageDealt = damage;

            return damage;
        }

        private bool IsCriticalHit(int attackPoints, Stat attackType)
        {
            float critChance = attackPoints / (float)(attackPoints + GetCritProtection(attackType));

            if (Random.value <= critChance) return true;
            else return false;
        }

        private int GetCritProtection(Stat attackType)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Defense);
        }

        public void ChangeAlignment(Alignment newAlignment)
        {
            entityManager.ChangeAlignment(this, alignment, newAlignment);
            alignment = newAlignment;
        }

        public bool HandleRaycast(GameObject player)
        {
            if (gameObject.tag == "Player") return false;

            Fighter playerFighter = player.GetComponent<Fighter>();
            if (!playerFighter.CanAttack(this)) return false;

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

        public Alignment GetAlignment()
        {
            return alignment;
        }
    }    
}
