using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] Weapon weaponPickup;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                other.gameObject.GetComponent<Fighter>().EquipWeapon(weaponPickup);
                Destroy(gameObject);
            }
        }
    }
}

