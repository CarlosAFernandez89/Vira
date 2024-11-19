#if UNITY_EDITOR
using GameplayAbilitySystem.GameplayEffects;
using UnityEditor;
using UnityEngine;

namespace GameplayAbilitySystem.Abilities
{
    [CustomEditor(typeof(GameplayAbilityBase))]
    public class AbilityBaseEditor : Editor
    {
        private string _tagInput; // Field to hold the user input for tags
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameplayAbilityBase gameplayAbilityBase = (GameplayAbilityBase)target;
            
            EditorGUILayout.Space(); // Adds some space before the header
            EditorGUILayout.LabelField("Gameplay Effects", EditorStyles.boldLabel);
            
            // Add button to add applied effects
            if (GUILayout.Button("Add Applied Effect"))
            {
                gameplayAbilityBase.appliedEffects.Add(null); // Add a null entry for the user to assign an effect
            }

            // Display applied effects
            for (int i = 0; i < gameplayAbilityBase.appliedEffects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                gameplayAbilityBase.appliedEffects[i] = (GameplayEffectBase)EditorGUILayout.ObjectField("Applied Effect " + i, gameplayAbilityBase.appliedEffects[i], typeof(GameplayEffectBase), false);
                
                // Remove button for applied effects
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    gameplayAbilityBase.appliedEffects.RemoveAt(i);
                    EditorUtility.SetDirty(gameplayAbilityBase);
                }
                EditorGUILayout.EndHorizontal();
            }

            // Add button to add granted effects
            if (GUILayout.Button("Add Granted Effect"))
            {
                gameplayAbilityBase.grantedEffects.Add(null); // Add a null entry for the user to assign an effect
            }

            // Display granted effects
            for (int i = 0; i < gameplayAbilityBase.grantedEffects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                gameplayAbilityBase.grantedEffects[i] = (GameplayEffectBase)EditorGUILayout.ObjectField("Granted Effect " + i, gameplayAbilityBase.grantedEffects[i], typeof(GameplayEffectBase), false);
                
                // Remove button for granted effects
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    gameplayAbilityBase.grantedEffects.RemoveAt(i);
                    EditorUtility.SetDirty(gameplayAbilityBase);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(gameplayAbilityBase);
            }
        }
    }
}
#endif