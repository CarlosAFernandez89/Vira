using Character.UI;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Abilities.Progression
{
    [CreateAssetMenu(fileName = "GA_PlayerMenu", menuName = "Abilities/Character/Abilities/PlayerMenu")]
    public class GA_PlayerMenu : GameplayAbilityBase
    {
        [SerializeField] private PlayerMenu playerMenu;
        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            base.OnAbilityGranted(owningAbilitySystemComponent);

            var player = owningAbilitySystemComponent.gameObject;
            playerMenu = player.GetComponentInChildren<PlayerMenu>();
        }

        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);

            if (playerMenu != null)
            {
                playerMenu.TogglePlayerMenu();
            }
            
            EndAbility();
        }

        protected override void EndAbility()
        {
            base.EndAbility();
        }

        protected override void HandleAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                default:
                    break;
            }
        }
    }
}