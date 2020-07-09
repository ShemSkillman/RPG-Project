using UnityEngine;
using RPG.Saving;
using System;

namespace RPG.Stats
{
    // Used to evaluate character level
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] int totalXP;
        
        public event Action onExperienceGained;

        public object CaptureState()
        {
            return totalXP;
        }

        public void GainExperience(int experience)
        {
            totalXP += experience;
            onExperienceGained();
        }

        public void RestoreState(object state)
        {
            totalXP = (int)state;
        }

        public int GetTotalXP()
        {
            return totalXP;
        }
    }
}

