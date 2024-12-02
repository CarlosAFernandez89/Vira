using System;
using System.Collections;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using GameplayAbilitySystem.Abilities;
using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameplayAbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        // List of available abilities
    
        [NonSerialized]  public AttributesComponent attributesComponent;
        [SerializeField] public List<GameplayAbilityBase> availableAbilities = new List<GameplayAbilityBase>();
        [SerializeField] public List<GameplayEffectBase> gameplayEffects = new List<GameplayEffectBase>();

        // Dictionary to track cooldowns (ability name -> time remaining)
        private Dictionary<GameplayAbilityBase, float> _abilityCooldowns = new Dictionary<GameplayAbilityBase, float>();

        [Header("Gameplay Tags")]
        [SerializeField] protected internal GameplayTagContainer DefaultTags = new GameplayTagContainer();
        [SerializeField] protected internal GameplayTagContainer AppliedTags = new GameplayTagContainer();
        private GameplayTagContainer BlockedAbilities = new GameplayTagContainer();
        
        private readonly Dictionary<GameplayAbilityBase, System.Action<InputAction.CallbackContext>> _delegateHandlers = new();

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
                availableAbilities[i].OnAbilityGranted(this);
                BindInputAction(availableAbilities[i]);
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

        public Vector3 GetOwningActorLocation()
        {
            return transform.position;
        }
        
    
        /// <summary>
        /// Grants and ability and binds the input of a Gameplay Ability to the AbilitySystemComponent
        /// </summary>
        /// <param name="gameplayAbility">Reference to the owners AbilitySystemComponent</param>
        public void GrantAbility(GameplayAbilityBase gameplayAbility)
        {
            if (!availableAbilities.Contains(gameplayAbility))
            {
                availableAbilities.Add(gameplayAbility);
                BindInputAction(gameplayAbility);
                gameplayAbility.OnAbilityGranted(this);
            }
        }
    
        /// <summary>
        /// Removes the ability and unbinds the input of a Gameplay Ability to the AbilitySystemComponent
        /// </summary>
        /// <param name="gameplayAbility">Reference to the owners AbilitySystemComponent</param>
        public void RemoveAbility(GameplayAbilityBase gameplayAbility)
        {
            if (availableAbilities.Contains(gameplayAbility))
            {
                availableAbilities.Remove(gameplayAbility);
                UnbindInputAction(gameplayAbility);
                gameplayAbility.OnAbilityRemoved(this);
            }
        }
    
    
        /// <summary>
        /// Applies a gameplay effect to the AbilitySystemComponent
        /// </summary>
        /// <param name="effect">The effect to apply</param>
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

        /// <summary>
        /// Coroutine to handle the life cycle of the effect.
        /// Handles the lifetime and applying the actual effects to the attributes.
        /// Removes the effect when lifetime expires.
        /// </summary>
        /// <param name="effect">The effect to apply</param>
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
    
        /// <summary>
        /// Removes GameplayEffect from AbilitySystemComponent
        /// </summary>
        /// <param name="effect">The effect to apply</param>
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
        private void BindInputAction(GameplayAbilityBase gameplayAbility)
        {
            if (!gameplayAbility.activateOnGranted && gameplayAbility.inputAction != null)
            {
                var handler = new System.Action<InputAction.CallbackContext>(ctx => gameplayAbility.Activate(gameObject));
                _delegateHandlers[gameplayAbility] = handler;

                gameplayAbility.inputAction.action.Enable();
                gameplayAbility.inputAction.action.performed += handler;
            }
        }
    
        // Method to unbind input actions
        private void UnbindInputAction(GameplayAbilityBase gameplayAbility)
        {
            if (!gameplayAbility.activateOnGranted && gameplayAbility.inputAction != null && 
                _delegateHandlers.TryGetValue(gameplayAbility, out var handler))
            {
                gameplayAbility.inputAction.action.performed -= handler;
                gameplayAbility.inputAction.action.Disable();
                _delegateHandlers.Remove(gameplayAbility);
            }
        }

        // Attempts to activate an ability
        public bool TryActivateAbility(GameplayAbilityBase gameplayAbility)
        {
            // Check if ability exists in list and is off cooldown
            if (!availableAbilities.Contains(gameplayAbility) || IsAbilityOnCooldown(gameplayAbility)) return false;

            // Start cooldown
            StartCooldown(gameplayAbility);

            return true;
        }

        // Check if an ability is on cooldown
        private bool IsAbilityOnCooldown(GameplayAbilityBase gameplayAbility)
        {
            if (_abilityCooldowns.ContainsKey(gameplayAbility) && _abilityCooldowns[gameplayAbility] > 0)
            {
                //Debug.Log(ability.name + " is on cooldown.");
                return true;
            }

            return false;
        }
        

        // Start cooldown for an ability
        private void StartCooldown(GameplayAbilityBase gameplayAbility)
        {
            _abilityCooldowns[gameplayAbility] = gameplayAbility.cooldown;
            gameplayAbility.StartCooldown(gameObject);
            StartCoroutine(CooldownRoutine(gameplayAbility));
        }

        // Coroutine to handle cooldown timing
        private IEnumerator CooldownRoutine(GameplayAbilityBase gameplayAbility)
        {
            float remainingCooldown = gameplayAbility.cooldown;

            while (remainingCooldown > 0)
            {
                // Update the remaining cooldown time
                remainingCooldown -= Time.deltaTime;
            
                // Update the dictionary with the new remaining cooldown value
                _abilityCooldowns[gameplayAbility] = Mathf.Max(remainingCooldown, 0); // Ensure it doesn't go negative
            
                // Yield until the next frame
                yield return null; // This will pause the coroutine until the next frame
            }

            // Cooldown is complete; reset to zero
            remainingCooldown = 0;

            // Call the cooldown end event
            gameplayAbility.EndCooldown(gameObject); // Notify that cooldown has ended
        }

        public void CancelAllAbilitiesWithGameplayTag(GameplayTag gameplayTag)
        {
            foreach (GameplayAbilityBase ability in availableAbilities)
            {
                foreach (GameplayTag tempTag in ability.AbilityTags.GetTags())
                {
                    if (tempTag == gameplayTag)
                    {
                        ability.ForceEndAbility();
                    }
                }
            }
        }

        public bool HasRequiredGameplayTags(GameplayTagContainer gameplayTagContainer)
        {
            // Iterate over each tag in the passed container
            foreach (GameplayTag requiredTag in gameplayTagContainer.GetTags())
            {
                // Check if the tag is not found in either DefaultTags or AppliedTags
                bool tagFound = false;

                // Check in DefaultTags
                foreach (GameplayTag defaultTag in DefaultTags.GetTags())
                {
                    if (requiredTag == defaultTag)
                    {
                        tagFound = true;
                        break; // Exit once the tag is found
                    }
                }

                // Check in AppliedTags if not found in DefaultTags
                if (!tagFound)
                {
                    foreach (GameplayTag appliedTag in AppliedTags.GetTags())
                    {
                        if (requiredTag == appliedTag)
                        {
                            tagFound = true;
                            break; // Exit once the tag is found
                        }
                    }
                }

                // If any required tag is not found, return false
                if (!tagFound)
                {
                    return false;
                }
            }

            // If all required tags are found, return true
            return true;
        }

        public bool HasBlockedGameplayTags(GameplayTagContainer gameplayTagContainer)
        {
            // Iterate over each tag in the passed container
            foreach (GameplayTag blockedTag in gameplayTagContainer.GetTags())
            {
                // Check if the tag is found in either DefaultTags or AppliedTags
                foreach (GameplayTag defaultTag in DefaultTags.GetTags())
                {
                    if (blockedTag == defaultTag)
                    {
                        return false; // Return false immediately if any tag is found
                    }
                }

                foreach (GameplayTag appliedTag in AppliedTags.GetTags())
                {
                    if (blockedTag == appliedTag)
                    {
                        return false; // Return false immediately if any tag is found
                    }
                }
            }

            // If none of the blocked tags are found, return true
            return true;
        }
        
        public bool IsAbilityBlocked(GameplayTagContainer tags)
        {
            foreach (GameplayTag gameplayTag in BlockedAbilities.GetTags())
            {
                foreach (GameplayTag gameplayTag1 in tags)
                {
                    if (gameplayTag1.Equals(gameplayTag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public void AddBlockedAbilities(GameplayTagContainer tags)
        {
            BlockedAbilities.AddTags(tags);
        }

        public void RemoveBlockedAbilities(GameplayTagContainer tags)
        {
            BlockedAbilities.RemoveTags(tags);
        }
    }
}
