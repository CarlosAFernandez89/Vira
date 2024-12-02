using System;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using GameplayAbilitySystem.Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.BasicAbilities
{
    [CreateAssetMenu(fileName = "EnemyDeath", menuName = "Abilities/Enemy/Death")]
    public class GA_EnemyDeath : GameplayAbilityBase
    {
        private AbilitySystemComponent _abilitySystemComponent;
        [SerializeField] private GameObject soulPrefab;

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
            DropEnemySouls();
            
            EndAbility();
        }

        protected override void EndAbility()
        {
            base.EndAbility();
            //AbilitySystemLogger.Log("On Death End!");
            Destroy(_abilitySystemComponent.gameObject);
        }

        private void DropEnemySouls()
        {
            AttributesComponent attributesComponent = _abilitySystemComponent.attributesComponent;
            AttributeBase souls = attributesComponent.GetAttribute("Souls");
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            for (int i = 0; i < souls.BaseValue; ++i)
            {
                //Instantiate souls.
                Vector3 spawnLocation = _abilitySystemComponent.GetOwningActorLocation();
                Instantiate(soulPrefab, spawnLocation, Quaternion.identity).GetComponent<EnemySoul>().Initialize(player);
            }
        }

        protected override void HandleAnimationEvent(string eventName)
        {
            
        }
    }
}
