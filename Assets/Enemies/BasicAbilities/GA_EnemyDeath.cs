using System;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using GameplayAbilitySystem.Attributes;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemies.BasicAbilities
{
    [CreateAssetMenu(fileName = "EnemyDeath", menuName = "Abilities/Enemy/Death")]
    public class GA_EnemyDeath : GameplayAbilityBase
    {
        private AbilitySystemComponent _abilitySystemComponent;

        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            if (owningAbilitySystemComponent != null)
            {
                _abilitySystemComponent = owningAbilitySystemComponent;
                AttributeBase healthAttribute = owningAbilitySystemComponent.attributesComponent.GetAttribute("Health");
                healthAttribute.SetOnZeroReached(OnDeath);
            }
        }

        private void OnDeath()
        {
            string objectName = _abilitySystemComponent.gameObject.name;
            AbilitySystemLogger.Log($"{objectName} has died. On Death Start!");
            StartAbility(_abilitySystemComponent.gameObject);
        }

        protected override void StartAbility(GameObject user)
        {
            base.StartAbility(user);
            
            // Do whatever needs to be done (ie. Effects, etc.)
            
            EndAbility();
        }

        protected override void EndAbility()
        {
            base.EndAbility();
            //AbilitySystemLogger.Log("On Death End!");
            Destroy(_abilitySystemComponent.gameObject);
        }
    }
}
