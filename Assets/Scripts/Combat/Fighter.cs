using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        // Configuration parameters
        [SerializeField] Transform rightHandTransform;
        [SerializeField] Transform leftHandTransform;
        [SerializeField] WeaponConfig defaultWeapon;

        bool isMovingCloser = false;
        bool isActive = false;

        CombatTarget target;
        float timeSinceLastAttack = Mathf.Infinity;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        Mover mover;
        Animator animator;
        BaseStats baseStats;
        ActionScheduler actionScheduler;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            baseStats = GetComponent<BaseStats>();
            actionScheduler = GetComponent<ActionScheduler>();
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(GetInitialCurrentWeapon);
        }

        private void Start()
        {
            currentWeapon.ForceInit();
        }

        private Weapon GetInitialCurrentWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public CombatTarget GetTarget()
        {
            return target;
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (!isActive || !CanAttack(target)) return;

            bool inRange = Vector3.Distance(transform.position, target.transform.position) <= currentWeaponConfig.GetWeaponRange();

            if (!inRange && !isMovingCloser || !IsComfortableRange() && isMovingCloser) // MOVE TO TARGET
            {
                isMovingCloser = true;
                mover.MoveTo(target.transform.position, 1f);
            }
            else // STOP AND ATTACK
            {
                isMovingCloser = false;
                mover.Cancel();
                AttackBehaviour();
            }
        }

        private bool IsComfortableRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < (currentWeaponConfig.GetWeaponRange() * 0.75f);  
        }

        private void AttackBehaviour()
        {
            LookAtTarget();
            if (timeSinceLastAttack >= currentWeaponConfig.GetTimeBetweenAttacks())
            {
                // This will trigger the Hit() event
                timeSinceLastAttack = 0f;
                TriggerAttackAnimation();
            }
        }

        private void LookAtTarget()
        {
            var lookPos = target.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
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

            AttackPayload attackPayload = new AttackPayload(baseStats, currentWeaponConfig);
            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(target, rightHandTransform, leftHandTransform, attackPayload);
            }
            else
            {
                target.HandleAttack(attackPayload);
            }

            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnAttack();
            }
        }


        // Animation event
        private void Shoot()
        {
            Hit();
        }

        public bool StartAttackAction(CombatTarget combatTarget, int actionPriority)
        {
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Attack)) return false;

            target = combatTarget;
            isActive = true;

            actionScheduler.onStartAction?.Invoke();
            return true;
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
                if (isActive) AttackComplete();
                return false;
            }
        }

        private void AttackComplete()
        {
            actionScheduler.CancelCurrentAction();
        }

        public void Cancel()
        {
            isActive = false;
            target = null;
            mover.Cancel();
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        public IEnumerable<int> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Strength && !currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePoints();
            }
            else if (stat == Stat.Range && currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePoints();
            }
        }

        public IEnumerable<int> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Strength && !currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePercentage();
            }
            else if (stat == Stat.Range && currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePercentage();
            }
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;        
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);
        }
    }
}

