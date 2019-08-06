using UnityEngine;
using RPG.Saving;
using System;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] int experiencePoints;
        
        public event Action onExperienceGained;

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void GainExperience(int experience)
        {
            experiencePoints += experience;
            onExperienceGained();
        }

        public void RestoreState(object state)
        {
            experiencePoints = (int)state;
        }

        public int GetExperience()
        {
            return experiencePoints;
        }
    }
}

