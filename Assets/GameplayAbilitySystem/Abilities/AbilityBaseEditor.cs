using GameplayAbilitySystem.GameplayEffects;
using UnityEditor;
using UnityEngine;

namespace GameplayAbilitySystem.Abilities
{
    [CustomEditor(typeof(AbilityBase))]
    public class AbilityBaseEditor : Editor
    {
        private string _tagInput; // Field to hold the user input for tags
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AbilityBase abilityBase = (AbilityBase)target;
            
            // Gameplay Tags header
            EditorGUILayout.Space(); // Adds some space before the header
            EditorGUILayout.LabelField("Gameplay Tags", EditorStyles.boldLabel);

            // Tag input field
            _tagInput = EditorGUILayout.TextField("Tag Input", _tagInput);

            // Button to add the tag from the input field
            if (GUILayout.Button("Add Tag"))
            {
                if (!string.IsNullOrWhiteSpace(_tagInput)) // Ensure the input is not empty
                {
                    abilityBase.AddTag(_tagInput);
                    EditorUtility.SetDirty(this);
                    _tagInput = ""; // Clear input field after adding
                }
                else
                {
                    Debug.LogWarning("Tag input cannot be empty."); // Optional warning
                }
            }

            // Button to remove the tag from the input field
            if (GUILayout.Button("Remove Tag"))
            {
                if (!string.IsNullOrWhiteSpace(_tagInput)) // Ensure the input is not empty
                {
                    abilityBase.RemoveTag(_tagInput);
                    EditorUtility.SetDirty(this);
                    _tagInput = ""; // Clear input field after removing
                }
                else
                {
                    Debug.LogWarning("Tag input cannot be empty."); // Optional warning
                }
            }
            
            // Show all tags currently on the ability
            EditorGUILayout.Space(); // Adds some space before the tags section
            EditorGUILayout.LabelField("Owned Tags:", EditorStyles.label);

            // Print current tags
            if (abilityBase.gameplayTag != null)
            {
                string currentTags = string.Join(", ", abilityBase.gameplayTag.GetAllTags()); // Assuming you have a method GetAllTags() that retrieves all tags
                EditorGUILayout.TextArea(currentTags, GUILayout.Height(50)); // Show the tags in a text area for better visibility
            }
            else
            {
                EditorGUILayout.HelpBox("No tags available.", MessageType.Info);
            }
            
            // Button to clear all tags with confirmation
            if (GUILayout.Button("Clear All Tags"))
            {
                if (EditorUtility.DisplayDialog("Clear All Tags", "Are you sure you want to clear all tags?", "Yes", "No"))
                {
                    if (abilityBase.gameplayTag != null)
                        abilityBase.gameplayTag.ClearAllTags(); // Assuming you have a method to clear all tags
                    EditorUtility.SetDirty(abilityBase); // Mark the current instance as dirty
                }
            }
            
            
            EditorGUILayout.Space(); // Adds some space before the header
            EditorGUILayout.LabelField("Gameplay Effects", EditorStyles.boldLabel);
            
            // Add button to add applied effects
            if (GUILayout.Button("Add Applied Effect"))
            {
                abilityBase.appliedEffects.Add(null); // Add a null entry for the user to assign an effect
            }

            // Display applied effects
            for (int i = 0; i < abilityBase.appliedEffects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                abilityBase.appliedEffects[i] = (GameplayEffectBase)EditorGUILayout.ObjectField("Applied Effect " + i, abilityBase.appliedEffects[i], typeof(GameplayEffectBase), false);
                
                // Remove button for applied effects
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    abilityBase.appliedEffects.RemoveAt(i);
                    EditorUtility.SetDirty(abilityBase);
                }
                EditorGUILayout.EndHorizontal();
            }

            // Add button to add granted effects
            if (GUILayout.Button("Add Granted Effect"))
            {
                abilityBase.grantedEffects.Add(null); // Add a null entry for the user to assign an effect
            }

            // Display granted effects
            for (int i = 0; i < abilityBase.grantedEffects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                abilityBase.grantedEffects[i] = (GameplayEffectBase)EditorGUILayout.ObjectField("Granted Effect " + i, abilityBase.grantedEffects[i], typeof(GameplayEffectBase), false);
                
                // Remove button for granted effects
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    abilityBase.grantedEffects.RemoveAt(i);
                    EditorUtility.SetDirty(abilityBase);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(abilityBase);
            }
        }
    }
}