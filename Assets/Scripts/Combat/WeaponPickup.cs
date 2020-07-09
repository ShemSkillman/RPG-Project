using System.Collections;
using UnityEngine;
using RPG.Control.Cursor;
using RPG.Attributes;
using RPG.Core;
using RPG.Movement;

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
            Pickup(other);
        }

        // Check player collision to register pickup
        private void Pickup(Collider other)
        {
            if (other.tag == "Player")
            {
                // Weapon pickup
                if (weaponPickup != null)
                {
                    other.gameObject.GetComponent<Fighter>().EquipWeapon(weaponPickup);
                }
                // Healing item
                else if (healthToRestore > 0)
                {
                    other.gameObject.GetComponent<Health>().Heal(healthToRestore);
                }

                StartCoroutine(HideForSeconds(respawnTime));
            }
        }

        // Player move action
        private void GoToPickup(GameObject player, ActionMarker waypointMarker, int priority)
        {
            if (player.GetComponent<Mover>().StartMoveAction(transform.position, 1f, priority))
            {
                ActionMarker marker = Instantiate(waypointMarker, transform.position, waypointMarker.transform.rotation);
                marker.SetMarker(player.GetComponent<ActionScheduler>(), null);
            }
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ActivatePickup(false);
            yield return new WaitForSeconds(seconds);
            ActivatePickup(true);
        }

        // Pickup when visible
        private void ActivatePickup(bool isActive)
        {
            pickupCollider.enabled = isActive;
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(isActive);
            }
        }

        public bool HandleRaycast(GameObject player, ActionMarker attackMarker, ActionMarker waypointMarker, int priority)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GoToPickup(player, waypointMarker, priority);
            }

            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}

