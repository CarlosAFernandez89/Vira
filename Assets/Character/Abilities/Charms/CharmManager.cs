using System;
using System.Collections.Generic;
using System.Linq;
using GameplayAbilitySystem;
using SaveLoad;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Abilities.Charms
{
    public class CharmManager : MonoBehaviour
    {
        private AbilitySystemComponent _ownerAbilitySystemComponent = null;
        [SerializeField] private List<CharmAbilityBase> ownedCharmAbilities = new List<CharmAbilityBase>();
        [SerializeField] private List<CharmAbilityBase> activeCharmAbilities = new List<CharmAbilityBase>();
        [SerializeField] private int defaultCharmSlots = 3;

        // Action to bind for UI events to trigger when adding/removing active charms.
        public event Action OnActiveCharmsModified;
        
        // Action to Save the CharmManager data whenever we modify its base variables
        public event Action OnCharmManagerModified;

        private void Awake()
        {
            OnCharmManagerModified += SaveCharmManager;
        }
        
        void Start()
        {
            _ownerAbilitySystemComponent = GetComponentInParent<AbilitySystemComponent>();
            if (_ownerAbilitySystemComponent != null)
            {
                //Load all active charms from save file.
                if (LoadCharmManager())
                {
                    //Apply equipped charms to player.
                    AddActiveCharmsToPlayer();
                }
            }
        }

        private void SaveCharmManager()
        {
            SaveCharmData saveCharmData = new SaveCharmData(ownedCharmAbilities, activeCharmAbilities, defaultCharmSlots);
            SaveProfile<SaveCharmData> saveProfile = new SaveProfile<SaveCharmData>("SaveCharmData", saveCharmData);
            SaveManager.Save(saveProfile, SaveManager.GetActiveSaveSlot());
        }
        
        private bool LoadCharmManager()
        {
            SaveProfile<SaveCharmData> loadedProfile = SaveManager.Load<SaveCharmData>("SaveCharmData", SaveManager.GetActiveSaveSlot());
            if (loadedProfile != null)
            {
                var (ownedCharms, activeCharms) = loadedProfile.saveData.LoadCharms();
                
                ownedCharmAbilities = ownedCharms;
                activeCharmAbilities = activeCharms;
            
                return true;
            }

            return false;
        }

        private void AddActiveCharmsToPlayer()
        {
            if (_ownerAbilitySystemComponent == null) return;
            
            foreach (CharmAbilityBase charm in activeCharmAbilities)
            {
                _ownerAbilitySystemComponent.GrantAbility(charm);
            }
        }

        public bool GrantCharmAbility(CharmAbilityBase charmAbility)
        {
            if (_ownerAbilitySystemComponent == null) return false;
            if (ownedCharmAbilities.Contains(charmAbility)) return false;
            
            ownedCharmAbilities.Add(charmAbility);
            OnCharmManagerModified?.Invoke();
            return true;
        }

        public bool ActivateOwnedCharmAbility(string charmAbilityName)
        {
            if (_ownerAbilitySystemComponent == null) return false;

            foreach (var charmAbility in 
                     ownedCharmAbilities.Where(charmAbility => charmAbility.CharmInfo.DisplayName == charmAbilityName))
            {
                // Can't add another charm of the same kind.
                if(activeCharmAbilities.Contains(charmAbility)) return false;
                
                activeCharmAbilities.Add(charmAbility);
                OnActiveCharmAbilityAdded(charmAbility);
                
                OnActiveCharmsModified?.Invoke();
                OnCharmManagerModified?.Invoke();

                return true;
            }

            return false;
        }

        private void OnActiveCharmAbilityAdded(CharmAbilityBase charmAbility)
        {
            foreach (var gameplayEffect in charmAbility.charmEffects)
                _ownerAbilitySystemComponent.ApplyEffect(gameplayEffect);
        }

        public bool DeactivateOwnedCharmAbility(string charmAbilityName)
        {
            if (_ownerAbilitySystemComponent == null) return false;

            foreach (CharmAbilityBase charmAbilityBase in
                     activeCharmAbilities.Where(charmAbility => charmAbility.CharmInfo.DisplayName == charmAbilityName))
            {
                activeCharmAbilities.Remove(charmAbilityBase);
                OnActiveCharmAbilityRemoved(charmAbilityBase);
                
                OnActiveCharmsModified?.Invoke();
                OnCharmManagerModified?.Invoke();

                return true;
            }

            return false;
        }
        
        private void OnActiveCharmAbilityRemoved(CharmAbilityBase charmAbility)
        {
            foreach (var gameplayEffect in charmAbility.charmEffects)
            {
                _ownerAbilitySystemComponent.RemoveEffect(gameplayEffect);
            }
        }

        private void IncreaseActiveCharmTotal()
        {
            defaultCharmSlots++;
            OnCharmManagerModified?.Invoke();
        }

        private void DecreaseActiveCharmTotal()
        {
            defaultCharmSlots--;
            OnCharmManagerModified?.Invoke();
        }
        
    }
}
