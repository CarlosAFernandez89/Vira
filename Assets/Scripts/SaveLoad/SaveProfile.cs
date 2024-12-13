using System.Collections.Generic;
using System.Linq;
using Character.Abilities.Charms;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaveLoad
{
    [System.Serializable]
    public sealed class SaveProfile<T> where T : SaveProfileData
    {
        public string profileName;
        public T saveData;
        
        public SaveProfile(){}

        public SaveProfile(string profileName, T saveData)
        {
            this.profileName = profileName;
            this.saveData = saveData;
        }
    }
    
    public abstract record SaveProfileData {}

    public record SaveCharmData : SaveProfileData
    {
        public List<string> OwnedCharmAbilityPaths;
        public List<string> ActiveCharmAbilityPaths;
        public int ActiveCharmTotal;

        public SaveCharmData()
        {
            OwnedCharmAbilityPaths = new List<string>();
            ActiveCharmAbilityPaths = new List<string>();
            ActiveCharmTotal = 3; //Default to 3.
        }

        public SaveCharmData(List<CharmAbilityBase> ownedCharms, List<CharmAbilityBase> activeCharms, int charmTotal)
        {
        #if UNITY_EDITOR
            OwnedCharmAbilityPaths = ownedCharms
                .Select(charm => UnityEditor.AssetDatabase.GetAssetPath(charm))
                .ToList();
            ActiveCharmAbilityPaths = activeCharms
                .Select(charm => UnityEditor.AssetDatabase.GetAssetPath(charm))
                .ToList();
        #else
            OwnedCharmAbilityPaths = ownedCharms
                .Select(charm => charm.name)
                .ToList();
            ActiveCharmAbilityPaths = activeCharms
                .Select(charm => charm.name)
                .ToList();
        #endif
        
            ActiveCharmTotal = charmTotal;
        }
        
        public (List<CharmAbilityBase> ownedCharms, List<CharmAbilityBase> activeCharms) LoadCharms()
        {
            List<CharmAbilityBase> allOwnedCharms;
            List<CharmAbilityBase> allActiveCharms;

        #if UNITY_EDITOR
            // Load charms using AssetDatabase in the editor
            allOwnedCharms = OwnedCharmAbilityPaths
                .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<CharmAbilityBase>(path))
                .Where(charm => charm != null)
                .ToList();

            allActiveCharms = ActiveCharmAbilityPaths
                .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<CharmAbilityBase>(path))
                .Where(charm => charm != null)
                .ToList();
        #else
            // Load charms by name at runtime using Resources.Load
            allOwnedCharms = OwnedCharmAbilityPaths
                .Select(name => Resources.Load<CharmAbilityBase>($"Charms/{name}")) // Assuming charms are in Resources/Charms
                .Where(charm => charm != null)
                .ToList();
            
            allActiveCharms = ActiveCharmAbilityPaths
                .Select(name => Resources.Load<CharmAbilityBase>($"Charms/{name}"))
                .Where(charm => charm != null)
                .ToList();
        #endif

            return (allOwnedCharms, allActiveCharms);
        }
    }
}
