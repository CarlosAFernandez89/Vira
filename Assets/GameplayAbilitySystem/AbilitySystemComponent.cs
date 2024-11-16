using System;
using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.Abilities;
using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;

namespace GameplayAbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        // List of available abilities
    
        [NonSerialized]  public AttributesComponent attributesComponent;
        [SerializeField] public List<AbilityBase> availableAbilities = new List<AbilityBase>();
        [SerializeField] public List<GameplayEffectBase> gameplayEffects = new List<GameplayEffectBase>();

        // Dictionary to track cooldowns (ability name -> time remaining)
        private Dictionary<AbilityBase, float> _abilityCooldowns = new Dictionary<AbilityBase, float>();

        public GameplayTag GameplayTags { get; private set; } = new GameplayTag();

        private void Awake()
        {
            if (attributesComponent == null)
            {
                attributesComponent = GetComponent<AttributesComponent>();
            }
        }

        private void Start()
        {
            if (attributesComponent == null)
            {
                attributesComponent = GetComponent<AttributesComponent>();
            }
            
            if (attributesComponent != null)
            {
                attributesComponent.InitializeOnGameStart();
            }
            
            // Subscribe to ability events for each available ability
            for (int i = 0; i < availableAbilities.Count; ++i)
            {
                var clonedAbility = ScriptableObjectUtility.Clone(availableAbilities[i]);
                availableAbilities[i] = clonedAbility;
                BindInputAction(availableAbilities[i]);
                availableAbilities[i].OnAbilityGranted(this);
            }

            foreach (var gameplayEffect in gameplayEffects)
            {
                ApplyEffect(gameplayEffect);
            }
            
        }

        private void OnDestroy()
        {
            // Unsubscribe to ability events for each available ability
            foreach (var ability in availableAbilities)
            {
                UnbindInputAction(ability);
            }
        }
    
        // Method to grant an ability
        public void GrantAbility(AbilityBase ability)
        {
            if (!availableAbilities.Contains(ability))
            {
                availableAbilities.Add(ability);
                BindInputAction(ability);
                ability.OnAbilityGranted(this);
            }
        }
    
        // Method to unbind input actions when abilities are removed
        public void RemoveAbility(AbilityBase ability)
        {
            if (availableAbilities.Contains(ability))
            {
                availableAbilities.Remove(ability);
                UnbindInputAction(ability);
                ability.OnAbilityRemoved(this);
            }
        }
    
    
        public void ApplyEffect(GameplayEffectBase effect)
        {
            if (Mathf.Approximately(effect.duration, -1))
            {
                ModifyAttribute(effect);
            }
            else
            {
                StartCoroutine(HandleEffect(effect));
            }
        }

        private IEnumerator HandleEffect(GameplayEffectBase effect)
        {
            // Apply the effect immediately based on its attributes
            ModifyAttribute(effect);
        
            // Calculate the number of times to apply the effect based on the duration
            float timeElapsed = 0f;

            while (timeElapsed < effect.duration)
            {
                yield return new WaitForSeconds(effect.frequency);
                ModifyAttribute(effect);
                timeElapsed += effect.frequency;
            }

            // Logic for ending the effect goes here
            RemoveEffect(effect);
        }
    
        private void RemoveEffect(GameplayEffectBase effect)
        {
            // Logic to remove the effect from the target
            AttributeBase attributeBase = attributesComponent.GetAttribute(effect.attributeToModify);
            if (attributeBase != null)
            {
                attributeBase.ModifyCurrentValue(-effect.effectAmount);
            }
        }
    
        private void ModifyAttribute(GameplayEffectBase effect)
        {
            AttributeBase foundAttribute = attributesComponent.GetAttribute(effect.attributeToModify);
            if (foundAttribute != null)
            {
                switch (effect.valueToModify)
                {
                    case AttributeValue.Name: // We don't want to change the name
                        break;
                    case AttributeValue.BaseValue: foundAttribute.ModifyBaseValue(effect.effectAmount);
                        break;
                    case AttributeValue.CurrentValue: foundAttribute.ModifyCurrentValue(effect.effectAmount);
                        break;
                    case AttributeValue.MinValue: foundAttribute.ModifyMinValue(effect.effectAmount);
                        break;
                    case AttributeValue.MaxValue: foundAttribute.ModifyMaxValue(effect.effectAmount);
                        break;
                    default: AbilitySystemLogger.LogError("Invalid value for attribute: " + effect.name);
                        break;
                }
            }
        }
    
        // Method to bind input actions to abilities
        private void BindInputAction(AbilityBase ability)
        {
            if (!ability.activateOnGranted && ability.inputAction != null)
            {
                ability.inputAction.action.Enable();
                ability.inputAction.action.performed += ctx => ability.Activate(gameObject);
            }
        }
    
        // Method to unbind input actions
        private void UnbindInputAction(AbilityBase ability)
        {
            if (!ability.activateOnGranted && ability.inputAction != null)
            {
                ability.inputAction.action.performed -= ctx => ability.Activate(gameObject);
                ability.inputAction.action.Disable();
            }
        }

        // Attempts to activate an ability
        public bool TryActivateAbility(AbilityBase ability)
        {
            // Check if ability exists in list and is off cooldown
            if (!availableAbilities.Contains(ability) || IsAbilityOnCooldown(ability) || HasGameplayTag()) return false;

            // Start cooldown
            StartCooldown(ability);

            return true;
        }

        // Check if an ability is on cooldown
        private bool IsAbilityOnCooldown(AbilityBase ability)
        {
            if (_abilityCooldowns.ContainsKey(ability) && _abilityCooldowns[ability] > 0)
            {
                //Debug.Log(ability.name + " is on cooldown.");
                return true;
            }

            return false;
        }

        private bool HasGameplayTag()
        {
            return GameplayTags.HasTag("Ability");
        }

        // Start cooldown for an ability
        private void StartCooldown(AbilityBase ability)
        {
            _abilityCooldowns[ability] = ability.cooldown;
            ability.StartCooldown(gameObject);
            StartCoroutine(CooldownRoutine(ability));
        }

        // Coroutine to handle cooldown timing
        private IEnumerator CooldownRoutine(AbilityBase ability)
        {
            float remainingCooldown = ability.cooldown;

            while (remainingCooldown > 0)
            {
                // Update the remaining cooldown time
                remainingCooldown -= Time.deltaTime;
            
                // Update the dictionary with the new remaining cooldown value
                _abilityCooldowns[ability] = Mathf.Max(remainingCooldown, 0); // Ensure it doesn't go negative
            
                // Yield until the next frame
                yield return null; // This will pause the coroutine until the next frame
            }

            // Cooldown is complete; reset to zero
            remainingCooldown = 0;

            // Call the cooldown end event
            ability.EndCooldown(gameObject); // Notify that cooldown has ended
        }
    
    }
}
