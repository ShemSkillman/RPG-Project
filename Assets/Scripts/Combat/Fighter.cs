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

        Transform rightHandTransform, leftHandTransform;
        const string rightHandName = "Hand_R", leftHandName = "Hand_L";

        Coroutine currentAttackAction;
        CombatTarget target, myCombatTarget;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;
        float timeSinceLastAttack = Mathf.Infinity, weaponCoolDownTime = 0f;
        const float variance = 0.2f;

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

        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
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

        IEnumerator AttackActionProgress(float speedFraction)
        {
            bool isMovingCloser = false;

            while (!health.GetIsDead() && CanAttack(target))
            {
                bool inRange = Vector3.Distance(transform.position, target.transform.position) <= currentWeaponConfig.GetWeaponRange();

                if (!inRange && !isMovingCloser || !IsComfortableRange() && isMovingCloser) // MOVE TO TARGET
                {
                    isMovingCloser = true;
                    mover.MoveTo(target.transform.position, speedFraction);
                }
                else // STOP AND ATTACK
                {
                    isMovingCloser = false;
                    mover.Cancel();
                    AttackBehaviour();
                }

                yield return null;
            }        

            AttackComplete();
        }

        private bool IsComfortableRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < (currentWeaponConfig.GetWeaponRange() * 0.75f);  
        }

        private void AttackBehaviour()
        {
            LookAtTarget();
            if (ReadyToAttack())
            {
                // This will trigger the Hit() event
                TriggerAttackAnimation();
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

        public bool StartAttackAction(CombatTarget combatTarget, float moveSpeedFraction, int actionPriority)
        {
            if (!actionScheduler.StartAction(this, actionPriority, ActionType.Attack)) return false;

            target = combatTarget;

            if (currentAttackAction != null) StopCoroutine(currentAttackAction);
            currentAttackAction = StartCoroutine(AttackActionProgress(moveSpeedFraction));

            actionScheduler.onStartAction?.Invoke();
            return true;
        }

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

        public void Cancel()
        {
            if (currentAttackAction != null) StopCoroutine(currentAttackAction);
            target = null;
            mover.Stop();
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

