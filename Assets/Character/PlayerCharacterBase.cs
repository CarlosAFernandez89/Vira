using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using UnityEngine;

namespace Character
{
    public class PlayerCharacterBase : MonoBehaviour, IAbilitySystemComponent
    {

        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return GetComponent(typeof(AbilitySystemComponent)) as AbilitySystemComponent;
        }
        
        void Awake()
        {
            if (AbilityAnimationManager.Instance == null)
            {
                GameObject managerObject = new GameObject("AbilityAnimationManager");
                managerObject.AddComponent<AbilityAnimationManager>();
            }
            
            if (GetComponent<AbilityAnimationEventBroadcaster>() == null)
            {
                gameObject.AddComponent<AbilityAnimationEventBroadcaster>();
            }
        }
    }
}
