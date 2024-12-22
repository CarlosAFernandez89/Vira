using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Abilities.Combat
{
    [CreateAssetMenu(fileName = "Player Melee Attack", menuName = "Abilities/Character/MeleeAttack")]
    public class PlayerMeleeAttack : MeleeBase
    {
        [Header("Animations")]
        [SerializeField] private AnimationClip upAttack;
        [SerializeField] private AnimationClip downAttack;
        [SerializeField] private AnimationClip defaultAttack;

        protected override void StartAbility(GameObject user)
        {
            if (upAttack != null && downAttack != null && defaultAttack != null)
            {
                _animator = user.GetComponent<Animator>();
                AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                if (overrideController != null)
                {
                    // This name needs to be the name of the default animation added to the state not
                    // the name of the state itself.
                    AnimationClip directionAnimation = CalculateAnimationDirection();
                    overrideController["DudeMonster_MeleeAttack_01"] = directionAnimation;
                    
                    _animator.runtimeAnimatorController = overrideController;
                    _animator.SetBool(IsAbilityOverride, true);

                    // End ability after animation is done playing. 
                    // Can also end it from the animation with EndAbility event.
                    CoroutineRunner.Instance.StartRoutine(CallAfterAnimationDone(Mathf.Abs(directionAnimation.length), user));
                }
                
                ActivateAbility(user);  // Default to triggering main ability action
                ApplyCost(user);
            }
        }

        private AnimationClip CalculateAnimationDirection()
        {
            Vector2 inputDirection = movementInput.action.ReadValue<Vector2>();
            return inputDirection.y switch
            {
                // Threshold for "up"
                > 0.5f => upAttack,
                // Threshold for "down"
                < -0.5f => downAttack,
                _ => defaultAttack
            };
        }
    }
}