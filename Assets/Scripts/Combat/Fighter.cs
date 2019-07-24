using UnityEngine;
using RPG.Movement;
using RPG.Core;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        // Configuration parameters
        [SerializeField] Transform rightHandTransform;
        [SerializeField] Transform leftHandTransform;
        [SerializeField] Weapon defaultWeapon;

        private Health target;
        private float timeSinceLastAttack = Mathf.Infinity;
        private Weapon currentWeapon;

        private Mover mover;
        private Animator animator;

        private void Start()
        {
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            EquipWeapon(defaultWeapon);
        }

        public void EquipWeapon(Weapon weapon)
        {
            if (currentWeapon != null) currentWeapon.Remove();
            currentWeapon = weapon;
            currentWeapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null || target.GetIsDead()) return;

            bool outOfRange = Vector3.Distance(transform.position, target.transform.position) > currentWeapon.GetWeaponRange();
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
            if (timeSinceLastAttack >= currentWeapon.GetTimeBetweenAttacks())
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
            if (currentWeapon.HasProjectile())
            {
                currentWeapon.LaunchProjectile(target);
            }
            else
            {
                target.TakeDamage(currentWeapon.GetWeaponDamage());
            }     
        }

        // Animation event
        private void Shoot()
        {
            Hit();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public bool CanAttack(GameObject combatTarget)
        {
            Health targetHealth = combatTarget.GetComponent<Health>();
            if (targetHealth != null && 
                !targetHealth.GetIsDead())
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
    }
}

