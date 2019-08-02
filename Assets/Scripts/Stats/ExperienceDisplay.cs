using UnityEngine;
using UnityEngine.UI;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;
        Text experienceText;

        private void Awake()
        {
            experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
            experienceText = GetComponent<Text>();
        }

        private void Update()
        {
            experienceText.text = string.Format("XP: {0}", experience.GetExperience());
        }
    }
}

