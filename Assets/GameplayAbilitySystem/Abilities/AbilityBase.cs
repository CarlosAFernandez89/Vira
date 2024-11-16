using System;
using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GameplayAbilitySystem.Abilities
{
    [Serializable]
    public struct AbilityCost
    {
        public string attribute; // Reference to the attribute
        public float amount; // Amount of the cost

        public AbilityCost(string attribute, float amount)
        {
            this.attribute = attribute;
            this.amount = amount;
        }
    }
    
    public abstract class AbilityBase : ScriptableObject
    {
        private Animator _animator;
        private static readonly int IsAbilityOverride = Animator.StringToHash("isAbilityOverride");

        [Header("Base Ability Settings")]
        public InputActionReference inputAction;
        public bool activateOnGranted = false;
        public string abilityName;
        public float cooldown;
        public AbilityCost abilityCost;

        // Events for ability lifecycle that external systems can subscribe to
        public event Action<GameObject> OnAbilityStart;
        public event Action<GameObject> OnAbilityEnd;
        public event Action<GameObject> OnAbilityFail;
        public event Action<GameObject> OnAbilityCooldownStart;
        public event Action<GameObject> OnAbilityCooldownEnd;
        
        private AbilitySystemComponent _owningAbilitySystemComponent;
        private AttributesComponent _attributesComponents;
        
        public AnimationClip abilityAnimation;
        

        #region Gameplay Effects
        
        [Header("Effects")]
        [HideInInspector] public List<GameplayEffectBase> appliedEffects; // Effects applied to the target
        [HideInInspector] public List<GameplayEffectBase> grantedEffects; // Effects granted to the user

        public void ApplyEffects(GameObject user, GameObject target)
        {
            AbilitySystemComponent ownerASC = user.GetComponent<AbilitySystemComponent>();
            AbilitySystemComponent targetASC = target.GetComponent<AbilitySystemComponent>();
            if (targetASC != null)
            {
                foreach (var appliedEffect in appliedEffects)
                {
                    targetASC.ApplyEffect(appliedEffect);
                }

                foreach (var grantedEffect in grantedEffects)
                {
                    ownerASC.ApplyEffect(grantedEffect);
                }
            }
        }

        public void GrantEffects(GameObject user)
        {
            foreach (var effect in grantedEffects)
            {
                effect.ApplyEffect(user); // Assuming effects can also be applied to the user
            }
        }
        

        #endregion
        
        
        private void Awake()
        {
            // Initialize the GameplayTag container
            if (gameplayTag == null)
            {
                gameplayTag = new GameplayTag(); // Create a new instance if not assigned in the inspector
            }
            
        }

        public virtual void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            // Do some custom action when granting an ability. 
            // Override per ability.
        }

        public virtual void OnAbilityRemoved(AbilitySystemComponent owningAbilitySystemComponent)
        {
            
        }

        #region GameplayTags
        
        public GameplayTag gameplayTag;

        public void AddTag(string tag)
        {
            gameplayTag.AddTag(tag);
        }

        public void RemoveTag(string tag)
        {
            gameplayTag.RemoveTag(tag);
        }

        public bool HasTag(string tag)
        {
            return gameplayTag.HasTag(tag);
        }

        public bool HasAnyTag(List<string> tags)
        {
            return gameplayTag.HasAnyTag(tags);
        }

        public bool HasAllTags(List<string> tags)
        {
            return gameplayTag.HasAllTags(tags);
        }

        public void PrintTags()
        {
            gameplayTag.PrintAllTags();
        }

        #endregion

        

        // Method to check if the user has enough resources to activate the ability
        protected bool HasSufficientResources(GameObject user)
        {
            if (_owningAbilitySystemComponent == null)
            {
                _owningAbilitySystemComponent = user.GetComponent<AbilitySystemComponent>();
                _attributesComponents = _owningAbilitySystemComponent.attributesComponent;
            }

            if (_attributesComponents == null)
            {
                return false;
            }

            AttributeBase wantedAttribute = null;
 
            wantedAttribute = _attributesComponents.GetAttribute(abilityCost.attribute);
            
            if(wantedAttribute == null) return true; // No cost was given

            return wantedAttribute.CurrentValue >= (wantedAttribute.MinValue + abilityCost.amount);// Ensure to implement a 'cost' variable
        }
        
        // Public method to initiate the ability
        public void Activate(GameObject user)
        {
            if (CanActivate(user))
            {
                OnAbilityStart?.Invoke(user);
                GrantEffects(user);
                StartAbility(user);
            }
            else
            {
                OnAbilityFail?.Invoke(user);
            }
        }

        // Base methods for each lifecycle stage, which derived classes can override
        protected virtual void StartAbility(GameObject user)
        {
            if (abilityAnimation != null)
            {
                _animator = user.GetComponent<Animator>();
                AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                if (overrideController != null)
                {
                    overrideController["DudeMonster_MeleeAttack_01"] = abilityAnimation;
                    _animator.runtimeAnimatorController = overrideController;
                    _animator.SetBool(IsAbilityOverride, true);

                    // End ability after animation is done playing. 
                    CoroutineRunner.Instance.StartRoutine(CallAfterAnimationDone(Mathf.Abs(abilityAnimation.length), user));
                }
                
                ActivateAbility(user);  // Default to triggering main ability action
                ApplyCost(user);
            }
            else // if we dont have an animation set, we can just end the ability as soon as possible.
            {
                ActivateAbility(user);  // Default to triggering main ability action
                ApplyCost(user);
                EndAbility(user);
            }

        }

        protected virtual void ActivateAbility(GameObject user)
        {
            // Abstract core action of the ability, implemented by derived classes
        }

        protected virtual void EndAbility(GameObject user)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsAbilityOverride, false);
            }

            OnAbilityEnd?.Invoke(user);  // Signals that the ability has ended
        }

        protected virtual bool CanActivate(GameObject user)
        {
            // False when on cooldown.
            AbilitySystemComponent asc = user.GetComponent<AbilitySystemComponent>();
            
            return HasSufficientResources(user) && asc.TryActivateAbility(this);
        }

        protected virtual void ApplyCost(GameObject user)
        {
            if (_owningAbilitySystemComponent == null)
            {
                _owningAbilitySystemComponent = user.GetComponent<AbilitySystemComponent>();
                _attributesComponents = _owningAbilitySystemComponent.attributesComponent;
            }
            
            AttributeBase wantedAttribute = null;

            wantedAttribute = _attributesComponents.GetAttribute(abilityCost.attribute);

            if (wantedAttribute != null)
            {
                wantedAttribute.ModifyCurrentValue(-abilityCost.amount);
            }
        }

        public void StartCooldown(GameObject user)
        {
            OnAbilityCooldownStart?.Invoke(user);
        }

        public void EndCooldown(GameObject user)
        {
            OnAbilityCooldownEnd?.Invoke(user);
        }

        private IEnumerator CallAfterAnimationDone(float waitTime, GameObject user)
        {
            yield return new WaitForSeconds(waitTime);

            EndAbility(user);
        }

        private bool IsAnimationDone(Animator animator, string stateName, int layerIndex)
        {
            // Get the current Animator state info for the specified layer
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            // Check if the state name matches and if the normalized time is >= 1 (indicating completion)
            return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1;
        }
    }
}