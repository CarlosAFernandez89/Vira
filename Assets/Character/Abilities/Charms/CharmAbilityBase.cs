using System.Collections.Generic;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Abilities.Charms
{
    [System.Serializable]
    public struct CharmInfo
    {
        public string DisplayName;
        public Sprite Icon;
        public string Description;
        public bool DestroyOnDeath;
        public int EquipCost;

        private CharmInfo(string displayName, Sprite icon,string description, bool destroyOnDeath = false, int equipCost = 1)
        {
            DisplayName = displayName;
            Icon = icon;
            Description = description;
            DestroyOnDeath = destroyOnDeath;
            EquipCost = equipCost;
        }
    }
    
    [System.Serializable]
    public class CharmAbilityBase : GameplayAbilityBase
    {
        [Header("Charm Info")] 
        [SerializeField] public CharmInfo CharmInfo;
        [SerializeField] public List<GameplayEffectBase> charmEffects;
        public bool _equipped = false;

        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            base.OnAbilityGranted(owningAbilitySystemComponent);
            
            GrantCharmEffects();
            _equipped = true;
        }

        private void GrantCharmEffects()
        {
            AbilitySystemComponent asc = GetAbilitySystemComponent();
            if (asc == null) return;
            
            foreach (GameplayEffectBase charmEffect in charmEffects)
            {
                asc.ApplyEffect(charmEffect);
            }
        }

        public override void OnAbilityRemoved(AbilitySystemComponent owningAbilitySystemComponent)
        {
            RemoveCharmEffects();

            _equipped = false;
            
            base.OnAbilityRemoved(owningAbilitySystemComponent);
        }

        private void RemoveCharmEffects()
        {
            AbilitySystemComponent asc = GetAbilitySystemComponent();
            if (asc == null) return;
            
            foreach (GameplayEffectBase charmEffect in charmEffects)
            {
                asc.RemoveEffect(charmEffect);
            }
        }

        protected override void StartAbility(GameObject user)
        {
            base.StartAbility(user);
        }

        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);
        }

        protected override bool CanActivate(GameObject user)
        {
            return base.CanActivate(user);
        }

        protected override void ApplyCost(GameObject user)
        {
            base.ApplyCost(user);
        }

        protected override void EndAbility()
        {
            base.EndAbility();
        }
        

        protected override void HandleAnimationEvent(string eventName)
        {
            //No implementation required.
            //All charms should be passive abilities.
        }
    }
}
