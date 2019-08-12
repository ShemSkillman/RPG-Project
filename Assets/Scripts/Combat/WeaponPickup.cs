using System;
using System.Collections;
using UnityEngine;
using RPG.Control;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
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
                Pickup(other.GetComponent<Fighter>());
            }
        }

        private void Pickup(Fighter fighter)
        {
            fighter.EquipWeapon(weaponPickup);
            StartCoroutine(HideForSeconds(respawnTime));
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

        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(callingController.GetComponent<Fighter>());
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}

