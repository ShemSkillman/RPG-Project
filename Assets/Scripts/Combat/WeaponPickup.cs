using System.Collections;
using UnityEngine;
using RPG.Control.Cursor;
using RPG.Attributes;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weaponPickup;
        [SerializeField] int healthToRestore = 0;
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
                Pickup(other.gameObject);
            }
        }

        private void Pickup(GameObject player)
        {
            if(weaponPickup != null)
            {
                player.GetComponent<Fighter>().EquipWeapon(weaponPickup);
            }
            else if (healthToRestore > 0)
            {
                player.GetComponent<Health>().Heal(healthToRestore);
            }
            
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

        public bool HandleRaycast(GameObject player)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(player);
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}

