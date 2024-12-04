using System;
using System.Collections;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GameplayAbilitySystem.Abilities
{
    // Interface for abilities that can receive animation events
    public interface IAnimationEventListener
    {
        void OnAbilityAnimationEvent(string eventName);
    }

    // Class to track active ability instances
    public class AbilityAnimationManager : MonoBehaviour
    {
        // Singleton instance
        public static AbilityAnimationManager Instance { get; private set; }

        // Dictionary to track active abilities per game object
        private Dictionary<GameObject, HashSet<IAnimationEventListener>> _activeAbilities 
            = new Dictionary<GameObject, HashSet<IAnimationEventListener>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Register an active ability instance
        public void RegisterAbility(GameObject owner, IAnimationEventListener ability)
        {
            if (!_activeAbilities.ContainsKey(owner))
            {
                _activeAbilities[owner] = new HashSet<IAnimationEventListener>();
            }
            _activeAbilities[owner].Add(ability);
        }

        // Unregister an ability instance
        public void UnregisterAbility(GameObject owner, IAnimationEventListener ability)
        {
            if (_activeAbilities.ContainsKey(owner))
            {
                _activeAbilities[owner].Remove(ability);
                if (_activeAbilities[owner].Count == 0)
                {
                    _activeAbilities.Remove(owner);
                }
            }
        }

        // Broadcast animation event to all active abilities on an object
        public void BroadcastAnimationEvent(GameObject owner, string eventName)
        {
            if (_activeAbilities.TryGetValue(owner, out var abilities))
            {
                // Create a copy of the list to safely iterate over
                // Its possible that the abilities get removed during the foreach
                // and we don't want to lock it.
                var abilitiesCopy = new HashSet<IAnimationEventListener>(abilities);
                foreach (var ability in abilitiesCopy)
                {
                    ability.OnAbilityAnimationEvent(eventName);
                }
            }
        }
    }
    
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
    
    public abstract class GameplayAbilityBase : ScriptableObject, IAnimationEventListener
    {
        private Animator _animator;
        private static readonly int IsAbilityOverride = Animator.StringToHash("isAbilityOverride");

        [Header("Base Ability Settings")]
        [SerializeField] public InputActionReference inputAction;
        [SerializeField] public bool activateOnGranted = false;
        [SerializeField] public List<GameplayEffectBase> gameplayEffects;
        [SerializeField] public float cooldown;
        [SerializeField] public AbilityCost abilityCost;
        [SerializeField] protected bool endAbilityOnAnimationEnd = true;

        protected GameObject CurrentUser;
        private bool _abilityActive = false;

        // Events for ability lifecycle that external systems can subscribe to
        public event Action<GameObject> OnAbilityStart;
        public event Action OnAbilityEnd;
        public event Action<GameObject> OnAbilityFail;
        public event Action<GameObject> OnAbilityCooldownStart;
        public event Action<GameObject> OnAbilityCooldownEnd;
        
        private AbilitySystemComponent _owningAbilitySystemComponent;
        private AttributesComponent _attributesComponents;
        
        public AnimationClip abilityAnimation;
        
        [Header("Gameplay Tags")]
        [Tooltip(@"Tags for the ability. These tags are used when trying to activate abilities with certain tags " +
                 @"(ex: Activating an ability with the tags Ability.Death allows us to get it from the ability system " + 
                 @" instead of having to press a key bind or bind the activation to an event.)")]
        [SerializeField] protected internal GameplayTagContainer AbilityTags = new GameplayTagContainer();
        
        [Tooltip("Find all abilities that have these tags (in AbilityTags) and cancels them.")]
        [SerializeField] protected internal GameplayTagContainer CancelAbilitiesWithTag = new GameplayTagContainer();
        [Tooltip("Blocks activation of abilities with these tags while this ability is active")]
        [SerializeField] protected internal GameplayTagContainer BlockAbilitiesWithTag = new GameplayTagContainer();
        
        [Tooltip("Tags that are applied to the owner of this ability. Tags are added to the AbilitySystemComponent.AppliedTags.")]
        [SerializeField] protected internal GameplayTagContainer ActivationOwnedTags = new GameplayTagContainer();
        [Tooltip("Checks the owners AbilitySystemComponent for tags and only allows activation of ability if all tags are found.")]
        [SerializeField] protected internal GameplayTagContainer ActivationRequiredTags = new GameplayTagContainer();
        [Tooltip("Checks the owners AbilitySystemComponent for tags and blocks activation of ability if any are present.")]
        [SerializeField] protected internal GameplayTagContainer ActivationBlockedTags = new GameplayTagContainer();
        
        [SerializeField] protected internal GameplayTagContainer SourceRequiredTags = new GameplayTagContainer();
        [SerializeField] protected internal GameplayTagContainer SourceBlockedTags = new GameplayTagContainer();
        
        [Tooltip("Blocks the application of the gameplay effects if any of these are NOT found.")]
        [SerializeField] protected internal GameplayTagContainer TargetRequiredTags = new GameplayTagContainer();
        [Tooltip("Blocks the application of the gameplay effects if any of these tags are found.")]
        [SerializeField] protected internal GameplayTagContainer TargetBlockedTags = new GameplayTagContainer();

        
        protected AbilitySystemComponent GetAbilitySystemComponent()
        {
            return _owningAbilitySystemComponent;
        }

        #region Gameplay Effects
        
        [Header("Effects")]
        [HideInInspector] public List<GameplayEffectBase> appliedEffects; // Effects applied to the target
        [HideInInspector] public List<GameplayEffectBase> grantedEffects; // Effects granted to the user

        public void ApplyEffects(GameObject user, GameObject target)
        {
            AbilitySystemComponent ownerASC = user.GetComponent<AbilitySystemComponent>();
            AbilitySystemComponent targetASC = target.GetComponent<AbilitySystemComponent>();
            
            if (targetASC != null 
                && !targetASC.HasBlockedGameplayTags(TargetBlockedTags) 
                && targetASC.HasRequiredGameplayTags(TargetRequiredTags)
                )
            {
                foreach (var appliedEffect in appliedEffects)
                {
                    targetASC.ApplyEffect(appliedEffect);
                }
            }

            if (ownerASC != null 
                && !ownerASC.HasBlockedGameplayTags(SourceBlockedTags) 
                && ownerASC.HasRequiredGameplayTags(SourceRequiredTags)
                )
            {
                foreach (var grantedEffect in grantedEffects)
                {
                    ownerASC.ApplyEffect(grantedEffect);
                }
            }
        }

        private void GrantEffects(GameObject user)
        {
            foreach (var effect in grantedEffects)
            {
                effect.ApplyEffect(user);
            }
        }


        #endregion
        
        

        public virtual void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            // Set references for later use.
            _owningAbilitySystemComponent = owningAbilitySystemComponent;
            _attributesComponents = _owningAbilitySystemComponent.attributesComponent;
            
            // Do some custom action when granting an ability. 
            // Override per ability.
        }

        public virtual void OnAbilityRemoved(AbilitySystemComponent owningAbilitySystemComponent)
        {
            _owningAbilitySystemComponent = null;
            _attributesComponents = null;
        }
        

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
                // Cancels all abilities that have GameplayTags from "CancelAbilitiesWithTag"
                CancelAllRequiredAbilities(); 
                GetAbilitySystemComponent().AddBlockedAbilities(BlockAbilitiesWithTag);
                
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
            // Override the animation state to take in the abilities animation
            if (abilityAnimation != null)
            {
                _animator = user.GetComponent<Animator>();
                AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                if (overrideController != null)
                {
                    // This name needs to be the name of the default animation added to the state not
                    // the name of the state itself.
                    overrideController["DudeMonster_MeleeAttack_01"] = abilityAnimation;
                    
                    _animator.runtimeAnimatorController = overrideController;
                    _animator.SetBool(IsAbilityOverride, true);

                    // End ability after animation is done playing. 
                    // Can also end it from the animation with EndAbility event.
                    CoroutineRunner.Instance.StartRoutine(CallAfterAnimationDone(Mathf.Abs(abilityAnimation.length), user));
                }
            }
            
            ActivateAbility(user);  // Default to triggering main ability action
            ApplyCost(user);
        }

        protected virtual void ActivateAbility(GameObject user)
        {
            // Add Activation Owned Tags to the owningAbilitySystem
            AbilitySystemComponent asc = GetAbilitySystemComponent();
            if (asc != null)
            {
                foreach (var tag in ActivationOwnedTags)
                {
                    asc.AppliedTags.AddTag(tag);
                }

                CurrentUser = user;
                _abilityActive = true;
                AbilityAnimationManager.Instance.RegisterAbility(user, this);
            }
            
            // Abstract core action of the ability, implemented by derived classes
        }

        public void ForceEndAbility()
        {
            ForceEndAnimation();
            EndAbility();
        }

        protected virtual void EndAbility()
        {
            if (_abilityActive)
            {
                AbilitySystemComponent asc = GetAbilitySystemComponent();
                if (asc != null)
                {
                    foreach (var tag in ActivationOwnedTags)
                    {
                        asc.AppliedTags.RemoveTag(tag);
                    }

                    asc.RemoveBlockedAbilities(BlockAbilitiesWithTag);
                }

                if (_animator != null)
                {
                    _animator.SetBool(IsAbilityOverride, false);
                }

                if (CurrentUser != null)
                {
                    AbilityAnimationManager.Instance.UnregisterAbility(CurrentUser, this);
                }

                _abilityActive = false;
                CurrentUser = null;
                OnAbilityEnd?.Invoke(); // Signals that the ability has ended
            }
        }

        protected virtual bool CanActivate(GameObject user)
        {
            return HasSufficientResources(user) 
                   && _owningAbilitySystemComponent.TryActivateAbility(this) 
                   && _abilityActive == false
                   && CheckActivationRequiredGameplayTags()
                   && CheckActivationBlockedGameplayTags()
                   && !GetAbilitySystemComponent().IsAbilityBlocked(AbilityTags);
        }

        private bool CheckActivationRequiredGameplayTags()
        {
            if (ActivationRequiredTags.IsEmpty)
            {
                return true;
            }

            bool canActivate = true;

            foreach (GameplayTag requiredTag in ActivationRequiredTags)
            {
                bool tagFound = false;
                // Check if the required tag is found in the applied tags
                foreach (GameplayTag appliedTag in _owningAbilitySystemComponent.AppliedTags.GetTags())
                {
                    if (requiredTag == appliedTag)
                    {
                        tagFound = true;
                        break; // No need to check further once found
                    }
                }
                
                // Check if the required tag is found in the default tags
                foreach (GameplayTag appliedTag in _owningAbilitySystemComponent.DefaultTags.GetTags())
                {
                    if (requiredTag == appliedTag)
                    {
                        tagFound = true;
                        break; // No need to check further once found
                    }
                }

                // If any tag is not found, set canActivate to false and break
                if (!tagFound)
                {
                    canActivate = false;
                    break; // No need to continue checking once a missing tag is found
                }
            }

            return canActivate;
        }
        
        private bool CheckActivationBlockedGameplayTags()
        {
            // If there are no blocked tags, return true
            if (ActivationBlockedTags.IsEmpty)
            {
                return true;
            }

            // Loop through each blocked tag
            foreach (GameplayTag blockedTag in ActivationBlockedTags)
            {
                // Check if the blocked tag is found in the applied tags
                foreach (GameplayTag appliedTag in _owningAbilitySystemComponent.AppliedTags.GetTags())
                {
                    if (blockedTag == appliedTag)
                    {
                        // If tag is found, return false immediately
                        return false;
                    }
                }

                // Check if the blocked tag is found in the default tags
                foreach (GameplayTag defaultTag in _owningAbilitySystemComponent.DefaultTags.GetTags())
                {
                    if (blockedTag == defaultTag)
                    {
                        // If tag is found, return false immediately
                        return false;
                    }
                }
            }

            // If none of the blocked tags were found, return true
            return true;
        }

        
        private void CancelAllRequiredAbilities()
        {
            foreach (GameplayTag gameplayTag in CancelAbilitiesWithTag.GetTags())
            {
                GetAbilitySystemComponent().CancelAllAbilitiesWithGameplayTag(gameplayTag);
            }
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

            // We could have ended the ability another way already.
            if (_abilityActive && endAbilityOnAnimationEnd)
            {
                EndAbility();
            }
        }

        private bool IsAnimationDone(Animator animator, string stateName, int layerIndex)
        {
            // Get the current Animator state info for the specified layer
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            // Check if the state name matches and if the normalized time is >= 1 (indicating completion)
            return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1;
        }
        
        protected virtual void ForceEndAnimation()
        {
            if (_animator == null) return;

            // Stop the animation by resetting the Animator parameters
            _animator.SetBool(IsAbilityOverride, false);

            // Optionally, transition to an idle or default state
            _animator.Play("Idle"); // Replace "Idle" with the name of your default animation state

            // Reset the runtime animator controller if needed
            _animator.runtimeAnimatorController = _animator.runtimeAnimatorController;
        }


        // Implementation of IAnimationEventListener
        public virtual void OnAbilityAnimationEvent(string eventName)
        {
            if (!_abilityActive) return;
            
            // Handle the animation event in the derived ability class
            HandleAnimationEvent(eventName);
        }

        // Abstract method to be implemented by specific abilities
        protected abstract void HandleAnimationEvent(string eventName);
    }
    
    // Animation event broadcaster component
    public class AbilityAnimationEventBroadcaster : MonoBehaviour
    {
        // Called by Animation Events
        public void BroadcastAnimationEvent(string eventName)
        {
            AbilityAnimationManager.Instance.BroadcastAnimationEvent(gameObject, eventName);
        }
    }
}