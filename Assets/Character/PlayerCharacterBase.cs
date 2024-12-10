using System;
using Character.Abilities.Charms;
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

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.H))
            {
                var charms = gameObject.GetComponent<CharmManager>();
                if (charms != null)
                {
                    charms.ActivateOwnedCharmAbility("MaxHealthIncrease");
                }
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.J))
            {
                var charms = gameObject.GetComponent<CharmManager>();
                if (charms != null)
                {
                    charms.DeactivateOwnedCharmAbility("MaxHealthIncrease");
                }
            }
        }
    }
}
