using System;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Combat
{

    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField] Weapon weaponPrefab;
        [SerializeField] AnimatorOverrideController weaponAnimatorOverride;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] int bonusDamagePoints = 10;
        [Range(0, 100)]
        [SerializeField] int bonusDamagePercentage = 10;
        [SerializeField] float weaponWeight = 1f;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile;

        const string weaponName = "Weapon";
        const float minTimeBetweenAttacks = 1f;

        // Spawns weapon instance in avatar hand
        public Weapon Spawn(Transform rightHand, Transform leftHand, Animator animator)
        {
            DestroyOldWeapon(rightHand, leftHand);
            Weapon weapon = null;

            if (weaponPrefab != null)
            {
                Transform weaponHandTransform = GetHandTransform(rightHand, leftHand);
                weapon = Instantiate(weaponPrefab, weaponHandTransform);
                weapon.gameObject.name = weaponName;           
            }

            // Changes avatar animations to match held weapon
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (weaponAnimatorOverride != null)
            {
                animator.runtimeAnimatorController = weaponAnimatorOverride;
            }    
            else if (overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            return weapon;
        }

        // Destroys currently held weapon if present
        private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            Transform oldWeapon = rightHand.Find(weaponName);
            if (oldWeapon == null)
            {
                oldWeapon = leftHand.Find(weaponName);
            }

            if (oldWeapon == null) return;
            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);
        }

        private Transform GetHandTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform;
            if (isRightHanded) handTransform = rightHand;
            else handTransform = leftHand;
            return handTransform;
        }

        public float GetWeaponRange()
        {
            return weaponRange;
        }

        public float GetWeaponWeight()
        {
            return weaponWeight;
        }

        public int GetBonusDamagePoints()
        {
            return bonusDamagePoints;
        }

        public int GetBonusDamagePercentage()
        {
            return bonusDamagePercentage;
        }

        public float GetTimeBetweenAttacks()
        {
            return minTimeBetweenAttacks * weaponWeight;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(CombatTarget target, Transform rightHand, Transform leftHand, AttackPayload attackPayload)
        {
            Projectile projectileInstance = Instantiate(projectile, GetHandTransform(rightHand, leftHand).position, Quaternion.identity);
            projectileInstance.SetTarget(target, attackPayload);
        }
    }
}
