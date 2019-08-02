using UnityEngine;
using RPG.Saving;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] int experiencePoints;

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void GainExperience(int experience)
        {
            experiencePoints += experience;
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

