using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;
using System.Collections;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] WeaponConfig defaultWeapon;
        [SerializeField] float rotateToTargetSmoothing = 10f;
        [SerializeField] float moveToTargetRefreshTime = 1f;

        // State
        Coroutine currentAttackAction;
        CombatTarget target, myCombatTarget;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;
        float timeSinceLastAttack = Mathf.Infinity, weaponCoolDownTime = 0f;
        const float variance = 0.2f;

        // Used to position weapon prefab
        Transform rightHandTransform, leftHandTransform;
        const string rightHandName = "Hand_R", leftHandName = "Hand_L";

        // Cache references
        Mover mover;
        Animator animator;
        BaseStats baseStats;
        ActionScheduler actionScheduler;
        Health health;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            myCombatTarget = GetComponent<CombatTarget>();
            animator = GetComponent<Animator>();
            baseStats = GetComponent<BaseStats>();
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(GetInitialCurrentWeapon);
            FindHands();
        }

        private void Start()
        {
            currentWeapon.ForceInit();
        }        

        private Weapon GetInitialCurrentWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        // Gets hand transforms on character avatar
        private void FindHands()
        {
            Transform[] children = GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                if (child.gameObject.name == leftHandName)
                {
                    leftHandTransform = child;
                }
                else if (child.gameObject.name == rightHandName)
                {
                    rightHandTransform = child;
                }

                if (leftHandTransform != null && rightHandTransform != null) return;
            }
        }

        // Weapon attributes used by fighter
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        // Spawns weapon prefab
        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            // Random cooldown before weapon can be used
            RandomizeWeaponCoolDown();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public CombatTarget GetTarget()
        {
            return target;
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        // Progresses attack action to completion
        IEnumerator AttackActionProgress(float speedFraction)
        {
            while (!health.GetIsDead() && CanAttack(target))
            {
                // Move closer to target
                if (!IsComfortableRange())
                {
                    mover.MoveTo(target.transform.position, speedFraction);
                    yield return new WaitForSeconds(moveToTargetRefreshTime);
                }
                // Stop and attack
                else
                {
                    mover.Cancel();
                    AttackBehaviour();
                    yield return null;
                }

            }        

            AttackComplete();
        }

        // Target preferred to be under 75% of max range when ranged
        private bool IsComfortableRange()
        {
            if (currentWeaponConfig.HasProjectile())
                return Vector3.Distance(transform.position, target.transform.position) < (currentWeaponConfig.GetWeaponRange() * 0.75f);  
            else
            {
                return Vector3.Distance(transform.position, target.transform.position) < (currentWeaponConfig.GetWeaponRange());
            }
        }

        private void AttackBehaviour()
        {
            LookAtTarget();
            if (ReadyToAttack())
            {
                // This will trigger the Hit() event
                TriggerAttackAnimation();

                if (currentWeapon.value != null)
                {
                    currentWeapon.value.OnStartAttack();
                }
            }
        }

        private bool ReadyToAttack()
        {
            if (timeSinceLastAttack >= weaponCoolDownTime)
            {
                timeSinceLastAttack = 0f;
                RandomizeWeaponCoolDown();
                return true;
            }

            return false;
        }

        private void RandomizeWeaponCoolDown()
        {
            weaponCoolDownTime = currentWeaponConfig.GetTimeBetweenAttacks() * UnityEngine.Random.Range(1f - variance, 1f + variance);
        }

        private void LookAtTarget()
        {
            var lookPos = target.transform.position - transform.position;

            // Ignore if target is higher or lower
            lookPos.y = 0;

            var targetRotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateToTargetSmoothing);
        }

        private void TriggerAttackAnimation()
        {
            // Prevents immediate stop of triggered attack
            animator.ResetTrigger("stopAttack");
            animator.SetTrigger("attack");
        }

        // Animation event
        private void Hit()
        {
            if (target == null) return;

            AttackPayload attackPayload = new AttackPayload(baseStats, currentWeaponConfig);

            // Range weapon shoot
            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(target, rightHandTransform, leftHandTransform, attackPayload);
            }
            // Melee attack
            else
            {
                target.HandleAttack(attackPayload);
            }

            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnEndAttack();
            }
        }


        // Animation event
        private void Shoot()
        {
            Hit();
        }

        public bool StartAttackAction(CombatTarget newTarget, float moveSpeedFraction, int actionPriority)
        {
            // Check scheduler free or interruptable
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Attack)) return false;

            target = newTarget;

            // Start attack progress
            if (currentAttackAction != null) StopCoroutine(currentAttackAction);
            currentAttackAction = StartCoroutine(AttackActionProgress(moveSpeedFraction));

            actionScheduler.onStartAction?.Invoke();
            return true;
        }

        // Validates given target
        public bool CanAttack(CombatTarget combatTarget)
        {
            if (combatTarget != null && 
                !combatTarget.GetIsDead() &&
                combatTarget.GetClan() != myCombatTarget.GetClan())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AttackComplete()
        {
            actionScheduler.CancelCurrentAction();
        }

        // Stops attack in progress
        public void Cancel()
        {
            if (currentAttackAction != null) StopCoroutine(currentAttackAction);
            target = null;
            mover.Stop();
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        // Adds bonus to given stat
        public IEnumerable<int> GetAdditiveModifiers(Stat stat)
        {
            // Melee bonus (strength stat)
            if (stat == Stat.Strength && !currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePoints();
            }
            // Range bonus (range stat)
            else if (stat == Stat.Range && currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePoints();
            }
        }

        // Mutliplies given stat
        public IEnumerable<int> GetPercentageModifiers(Stat stat)
        {
            // Melee attack multiplier
            if (stat == Stat.Strength && !currentWeaponConfig.HasProjectile())
            {
                yield return currentWeaponConfig.GetBonusDamagePercentage();
            }
            // Range attack multiplier
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

