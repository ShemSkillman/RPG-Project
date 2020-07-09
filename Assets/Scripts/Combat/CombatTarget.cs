using RPG.Attributes;
using RPG.Stats;
using UnityEngine;
using RPG.Control.Cursor;
using RPG.Core;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        [SerializeField] Clan startingClan = Clan.None;
        Clan currentClan = Clan.None;

        // Cache references
        Health health;
        BaseStats baseStats;
        EntityManager entityManager;

        // Critical hit deals double damage
        const int criticalDamageMultiplier = 2;

        // Events
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

        // Called by enemy fighter
        public bool HandleAttack(AttackPayload attackPayload)
        {
            if (GetIsDead()) return false;

            CheckFriendlyFire(attackPayload);
            AttackReport attackReport = new AttackReport(AttackResult.Miss, attackPayload.instigator);
            
            // Check if miss
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

        // Determines whether attack was made by ally
        // Ally will become enemy if so
        private void CheckFriendlyFire(AttackPayload attackPayload)
        {
            CombatTarget aggressor = attackPayload.instigator.GetComponent<CombatTarget>();
            entityManager.EvaluateAttack(aggressor, this);
        }

        // Range attacks never miss when landed
        private bool IsHit(AttackPayload attackPayload)
        {
            if (attackPayload.attackType == Stat.Range) return true;

            // Calculate if melee attack hit
            int evasion = baseStats.GetStat(Stat.Swiftness) * 2;
            float hitChance = attackPayload.hitPrecision / (float)(attackPayload.hitPrecision + evasion);

            if (UnityEngine.Random.value <= hitChance) return true;
            else return false;
        }

        // Evaluates damage taken when hit
        private void CalculateDamage(AttackPayload attackPayload, AttackReport attackReport)
        {
            attackReport.result = AttackResult.Hit;
            
            // Defense reduces damage taken
            float damageReduction = attackPayload.attackPoints / (float)(baseStats.GetStat(Stat.Defense) + attackPayload.attackPoints);
            attackPayload.damage = Mathf.RoundToInt(attackPayload.damage * damageReduction);

            if (IsCriticalHit(attackPayload))
            {
                attackPayload.damage *= criticalDamageMultiplier;
                attackReport.result = AttackResult.CriticalHit;
            }

            attackReport.damageDealt = attackPayload.damage;
        }

        // Check if  an attack is critical
        private bool IsCriticalHit(AttackPayload attackPayload)
        {
            float critChance = (attackPayload.critStrike / (float)(attackPayload.critStrike + GetCritProtection(attackPayload.attackType)));

            if (UnityEngine.Random.value <= critChance) return true;
            else return false;
        }

        // Check if this entity can be attacked by player
        public bool HandleRaycast(GameObject player, ActionMarker attackMarker, ActionMarker waypointMarker, int priority)
        {
            Fighter playerFighter = player.GetComponent<Fighter>();

            if (!playerFighter.CanAttack(this) || tag == "Player" ||
                GetClan() == Clan.PlayerParty) return false;

            // Left click mouse button to attack this entity
            if (Input.GetMouseButtonDown(0))
            {
                playerFighter.StartAttackAction(this, 1f, priority);

                // Attack marker follows this entity
                ActionMarker marker = Instantiate(attackMarker, transform.position, attackMarker.transform.rotation);
                marker.SetMarker(player.GetComponent<ActionScheduler>(), transform);
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        // Crit protection reduces chances of recieving critical hit
        private int GetCritProtection(Stat attackType)
        {
            return baseStats.GetStat(attackType) + baseStats.GetStat(Stat.Defense);
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
