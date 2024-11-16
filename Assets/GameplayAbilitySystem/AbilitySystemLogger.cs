using UnityEngine;

namespace GameplayAbilitySystem
{
    public class AbilitySystemLogger : MonoBehaviour
    {

        public static void Log(string message)
        {
            Debug.Log($"AbilitySystem: " + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"AbilitySystem: " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError($"AbilitySystem: " + message);
        }
    }
}
