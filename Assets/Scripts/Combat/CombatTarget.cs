using RPG.Attributes;
using RPG.Stats;
using UnityEngine;
using RPG.Control.Cursor;
using System;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        Health health;
        BaseStats baseStats;
        EntityManager entityManager;

        const int criticalDamageMultiplier = 2;

        [SerializeField] Clan startingClan = Clan.None;
        Clan currentClan = Clan.None;

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
            entityManager.RegisterEntity(this, startingClan);
        }

        public bool HandleAttack(AttackPayload attackPayload)
        {
            if (GetIsDead()) return false;

            CheckFriendlyFire(attackPayload);
            AttackReport attackReport = new AttackReport(AttackResult.Miss);
            
            if (!IsHit(attackPayload))
            {
                onAttacked(attackReport);
                return false;
            }

            CalculateDamage(attackPayload, attackReport);
            health.TakeDamage(attackPayload.instigator, attackPayload.damage);

            if (GetIsDead())
            {
                attackReport.result = AttackResult.TargetDown;
                entityManager.RemoveEntity(this);
            }
            onAttacked(attackReport);

            return true;
        }

        private void CheckFriendlyFire(AttackPayload attackPayload)
        {
            CombatTarget aggressor = attackPayload.instigator.GetComponent<CombatTarget>();
            entityManager.EvaluateAttack(aggressor, this);
        }

        private bool IsHit(AttackPayload attackPayload)
        {
            if (attackPayload.attackType == Stat.Range) return true;

            int evasion = baseStats.GetStat(Stat.Swiftness) * 2;
            float hitChance = attackPayload.hitPrecision / (float)(attackPayload.hitPrecision + evasion);

            if (UnityEngine.Random.value <= hitChance) return true;
            else return false;
        }

        private void CalculateDamage(AttackPayload attackPayload, AttackReport attackReport)
        {
            attackReport.result = AttackResult.Hit;
            
            float damageReduction = attackPayload.attackPoints / (float)(baseStats.GetStat(Stat.Defense) + attackPayload.attackPoints);
            attackPayload.damage = Mathf.RoundToInt(attackPayload.damage * damageReduction);

            if (IsCriticalHit(attackPayload))
            {
                attackPayload.damage *= criticalDamageMultiplier;
                attackReport.result = AttackResult.CriticalHit;
            }

            attackReport.damageDealt = attackPayload.damage;
        }

        private bool IsCriticalHit(AttackPayload attackPayload)
        {
            float critChance = (attackPayload.critStrike / (float)(attackPayload.critStrike + GetCritProtection(attackPayload.attackType)));

            if (UnityEngine.Random.value <= critChance) return true;
            else return false;
        }

        private int GetCritProtection(Stat attackType)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Defense);
        }

        public bool HandleRaycast(GameObject player)
        {
            if (gameObject.tag == "Player" || currentClan == Clan.PlayerParty) return false;

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

        public Clan GetClan()
        {
            return currentClan;
        }

        public void SetClan(Clan newClan)
        {
            currentClan = newClan;
        }

        public void ChangeClan(Clan newClan)
        {
            entityManager.ChangeClan(this, newClan);
        }
    }    
}
