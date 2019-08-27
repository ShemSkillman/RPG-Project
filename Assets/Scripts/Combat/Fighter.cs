using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        // Configuration parameters
        [SerializeField] Transform rightHandTransform;
        [SerializeField] Transform leftHandTransform;
        [SerializeField] Weapon defaultWeapon;

        CombatTarget target;
        float timeSinceLastAttack = Mathf.Infinity;
        LazyValue<Weapon> currentWeapon;

        Mover mover;
        Animator animator;
        BaseStats baseStats;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            baseStats = GetComponent<BaseStats>();
            currentWeapon = new LazyValue<Weapon>(GetInitialCurrentWeapon);
        }

        private void Start()
        {
            currentWeapon.ForceInit();
        }

        private Weapon GetInitialCurrentWeapon()
        {
            AttachWeapon(defaultWeapon);
            return defaultWeapon;
        }

        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon.value = weapon;
            AttachWeapon(weapon);
        }

        private void AttachWeapon(Weapon weapon)
        {
            weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public CombatTarget GetTarget()
        {
            return target;
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null || target.GetIsDead()) return;

            bool outOfRange = Vector3.Distance(transform.position, target.transform.position) > currentWeapon.value.GetWeaponRange();
            if (outOfRange)
            {
                mover.MoveTo(target.transform.position, 1f);
            }
            else
            {
                mover.Cancel();
                AttackBehaviour();
            }
        }

        private void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack >= currentWeapon.value.GetTimeBetweenAttacks())
            {
                // This will trigger the Hit() event
                timeSinceLastAttack = 0f;
                TriggerAttackAnimation();
            }
        }

        private void TriggerAttackAnimation()
        {
            animator.ResetTrigger("stopAttack");
            animator.SetTrigger("attack");
        }

        // Animation event
        private void Hit()
        {
            if (target == null) return;

            AttackPayload attackPayload = new AttackPayload(baseStats, currentWeapon.value);
            if (currentWeapon.value.HasProjectile())
            {
                currentWeapon.value.LaunchProjectile(target, rightHandTransform, leftHandTransform, attackPayload);
            }
            else
            {
                target.HandleAttack(attackPayload);
            }

            CheckFriendlyFire();
        }

        private void CheckFriendlyFire()
        {
            CombatTarget myCombatTarget = GetComponent<CombatTarget>();
            if (myCombatTarget.GetAlignment() == target.GetAlignment())
            {
                myCombatTarget.ChangeAlignment(Alignment.Rogue);
            }
        }


        // Animation event
        private void Shoot()
        {
            Hit();
        }

        public void Attack(CombatTarget combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget;
        }

        public bool CanAttack(CombatTarget combatTarget)
        {
            if (combatTarget != null && 
                !combatTarget.GetIsDead())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Cancel()
        {
            mover.Cancel();
            target = null;
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        public IEnumerable<int> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Strength && !currentWeapon.value.HasProjectile())
            {
                yield return currentWeapon.value.GetBonusDamagePoints();
            }
            else if (stat == Stat.Range && currentWeapon.value.HasProjectile())
            {
                yield return currentWeapon.value.GetBonusDamagePoints();
            }
        }

        public IEnumerable<int> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Strength && !currentWeapon.value.HasProjectile())
            {
                yield return currentWeapon.value.GetBonusDamagePercentage();
            }
            else if (stat == Stat.Range && currentWeapon.value.HasProjectile())
            {
                yield return currentWeapon.value.GetBonusDamagePercentage();
            }
        }

        public object CaptureState()
        {
            return currentWeapon.value.name;        
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            Weapon weapon = UnityEngine.Resources.Load<Weapon>(weaponName);
            EquipWeapon(weapon);
        }
    }
}

