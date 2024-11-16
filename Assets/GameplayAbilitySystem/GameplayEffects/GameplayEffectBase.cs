using GameplayAbilitySystem.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameplayAbilitySystem.GameplayEffects
{
    [CreateAssetMenu(fileName = "NewGameplayEffect", menuName = "AbilitySystem/GameplayEffect")]
    public class GameplayEffectBase : ScriptableObject
    {
        public string attributeToModify;
        public AttributeValue valueToModify; // which of the values to modify.
        public float effectAmount; // The amount to modify
        public float duration = -1; // Duration of the effect in seconds. -1 for infinite duration
        public float frequency; // How often to apply the effect. 0 for a single application

        // Method to apply the effect to a target
        public void ApplyEffect(GameObject target)
        {
            AbilitySystemComponent asc = target.GetComponent<AbilitySystemComponent>();
            if (asc != null)
            {
                asc.ApplyEffect(this);
            }
        }
    }
}