using RPG.Core;
using UnityEngine;

namespace RPG.Combat
{

    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon")]
    public class Weapon : ScriptableObject
    {
        [SerializeField] GameObject weaponPrefab;
        [SerializeField] AnimatorOverrideController weaponAnimatorOverride;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] int weaponDamage = 10;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile;
        Transform weaponHandTransform;
        GameObject weaponInstance;

        public void Spawn(Transform rightHand, Transform leftHand, Animator animator)
        {
            if (weaponPrefab != null)
            {
                weaponHandTransform = GetHandTransform(rightHand, leftHand);
                weaponInstance = Instantiate(weaponPrefab, weaponHandTransform);
            }
            if (weaponAnimatorOverride != null)
            {
                animator.runtimeAnimatorController = weaponAnimatorOverride;
            }       
        }

        public void Remove()
        {
            Destroy(weaponInstance);
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

        public int GetWeaponDamage()
        {
            return weaponDamage;
        }

        public float GetTimeBetweenAttacks()
        {
            return timeBetweenAttacks;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(Health target)
        {
            Projectile projectileInstance = Instantiate(projectile, weaponHandTransform.position, Quaternion.identity);
            projectileInstance.SetTarget(target, weaponDamage);
        }
    }
}
