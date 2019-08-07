using System;
using System.Collections;
using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] Weapon weaponPickup;
        [SerializeField] float respawnTime = 5f;
        Collider pickupCollider;

        private void Awake()
        {
            pickupCollider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                other.gameObject.GetComponent<Fighter>().EquipWeapon(weaponPickup);
                StartCoroutine(HideForSeconds(respawnTime));
            }
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ActivatePickup(false);
            yield return new WaitForSeconds(seconds);
            ActivatePickup(true);
        }

        private void ActivatePickup(bool isActive)
        {
            pickupCollider.enabled = isActive;
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(isActive);
            }
        }
    }
}

