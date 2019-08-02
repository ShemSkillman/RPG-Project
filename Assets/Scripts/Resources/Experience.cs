using UnityEngine;

namespace RPG.Resources
{
    public class Experience : MonoBehaviour
    {
        [SerializeField] int experiencePoints;

        public void GainExperience(int experience)
        {
            experiencePoints += experience;
        }
    }
}

