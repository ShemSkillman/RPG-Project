using System;
using System.Collections.Generic;
using RPG.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Saving
{
    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] string uniqueIdentifier = "";
        static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        // Returns collection of saved game attributes for this entity
        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                // Key = script name
                state[saveable.GetType().ToString()] = saveable.CaptureState();
            }
            return state;
        }

        // Redistributes saved game attributes to resore game state
        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }

        // Only executes in the Unity editor
        // Does not execute on any other platform
#if UNITY_EDITOR
        private void Update() {
            if (Application.IsPlaying(gameObject)) return;

            // Only executes when script is in the scene
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("uniqueIdentifier");
            
            // Checks ID valid + unique
            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                // Generates new ID for this entity
                property.stringValue = System.Guid.NewGuid().ToString();

                // Serialized value changed directly
                // Otherwise Unity will overwrite the unique ID 
                serializedObject.ApplyModifiedProperties();
            }

            globalLookup[property.stringValue] = this;
        }
#endif

        // Checks ID is unique 
        private bool IsUnique(string candidate)
        {
            if (!globalLookup.ContainsKey(candidate)) return true;
            
            // Entity ID already in lookup
            if (globalLookup[candidate] == this) return true;

            // Replace destroyed entity
            if (globalLookup[candidate] == null)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            // Checks entity has ID that matches lookup
            if (globalLookup[candidate].GetUniqueIdentifier() != candidate)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            return false;
        }
    }
}