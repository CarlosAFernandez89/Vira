using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Character.Abilities.Charms;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Character.UI.Charms
{
    public class CharmsUIManager : MonoBehaviour, IPlayerSubMenu
    {
        [SerializeField] private int _maxEquippedSlots = 6;
        [SerializeField] public GameObject _player; // Reference to the player GameObject
        
        [SerializeField] private VisualTreeAsset charmItemAsset; // Reference to CharmItemUI.uxml
        
        private VisualElement _root;
        private VisualElement _equippedCharmsContainer;
        private VisualElement _topHalf;
        private ListView _allCharmsList;
        private Label _charmNameLabel;
        private VisualElement _charmImage;
        private Label _charmDescriptionLabel;

        private CharmManager _charmManager; // Reference to the CharmManager on the player
        private List<CharmAbilityBase> _allCharms = new List<CharmAbilityBase>();
        private int _notchesUsed = 0;
        public int _maxNotches;

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("content-viewport");
        }

        private void Start()
        {
            _charmManager = _player.GetComponent<CharmManager>();
            _maxNotches = _charmManager.GetMaxCharmSlots();
        }

        public void InitializeSubMenu()
        {
            _equippedCharmsContainer = _root.Q<VisualElement>("EquippedCharmsContainer");
            _topHalf = _root.Q<VisualElement>("EquippedCharms");
            
            _allCharmsList = _root.Q<ListView>("AllCharmsList");
            _charmNameLabel = _root.Q<Label>("CharmName");
            _charmImage = _root.Q<VisualElement>("CharmImage");
            _charmDescriptionLabel = _root.Q<Label>("CharmDescription");
            
            Debug.LogWarning("Initializing Charms UI ...");

            LoadAllCharms();

            InitializeEquippedCharmList();
            
            Debug.Log($"Equipped Charms Container Child Count: {_equippedCharmsContainer.childCount}");

            ClearCharmInfo();
            
            _root.MarkDirtyRepaint();
        }

        public void DeinitializeSubMenu()
        {
            // Any cleanup when the UI is hidden.
        }

        private void LoadAllCharms()
        {
            // Load all CharmAbilityBase ScriptableObjects from the Resources/Charms folder
            _allCharms = Resources.LoadAll<CharmAbilityBase>("Charms").ToList();
        }
        
        private VisualElement CreateCharmItem(CharmAbilityBase charm)
        {
            if (charmItemAsset == null) return null;
            
            VisualElement charmItem = charmItemAsset.Instantiate();
            VisualElement cItem = charmItem.Q<VisualElement>("CharmItem");
            VisualElement imageSlot = cItem.Q<VisualElement>("ImageSlot");
            Button button = cItem.Q<Button>("Button");

            if (charm != null && imageSlot != null && button != null)
            {
                // Set charm image
                imageSlot.AddToClassList("charm-image");
                imageSlot.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);

                // Add hover behavior
                charmItem.RegisterCallback<MouseEnterEvent>(evt => DisplayCharmInfo(charm));
                charmItem.RegisterCallback<MouseLeaveEvent>(evt => ClearCharmInfo());

                // Add unequip button behavior
                button.clicked += () =>
                {
                    UnequipCharm(charm);
                    RefreshUI(); // Refresh the UI to update the lists
                };
            }
            else
            {
                // Empty slot
                if (imageSlot != null)
                {
                    imageSlot.AddToClassList("empty-slot");
                }

                // Clear hover behavior
                charmItem.UnregisterCallback<MouseEnterEvent>(evt => { });
                charmItem.UnregisterCallback<MouseLeaveEvent>(evt => { });

                // Clear button behavior
                if (button != null) button.clicked += () => { };
            }

            return charmItem;
        }
        

        private void InitializeEquippedCharmList()
        {
            Label label = _topHalf.Q<Label>("TopLabel");
            label.text = "Equipped Charms";
            
            Debug.Log("Initializing equipped charm list...");
            // Clear existing children
            _equippedCharmsContainer.Clear();

            // Get equipped charms from CharmManager
            List<CharmAbilityBase> equippedCharms = _charmManager.GetActiveCharmAbilities();

            // Add equipped charms
            for (int i = 0; i < _maxEquippedSlots; i++)
            {
                CharmAbilityBase charm = i < equippedCharms.Count ? equippedCharms[i] : null;
                Debug.Log($"Adding charm slot {i}: {charm?.CharmInfo.DisplayName ?? "Empty"}");
                _equippedCharmsContainer.Add(CreateCharmItem(charm));
            }
        }

        private void InitializeAllCharmsList()
        {
            // Set the item height for the ListView
            _allCharmsList.fixedItemHeight = 64;

            // Assign the methods to the ListView
            List<CharmAbilityBase> charmList = _allCharms;
            if (charmList != null && charmList.Count > 0)
            {
                _allCharmsList.itemsSource = charmList;
            }
            else
            {
                Debug.LogError("AllCharmsListEmpty");
            }
            
            _allCharmsList.makeItem = () =>
            {
                Debug.Log("Creating new all charm list");
                VisualElement charmItem = charmItemAsset.Instantiate();
                charmItem.Q<VisualElement>("ImageSlot").AddToClassList("empty-slot");
                return charmItem;
            };
            
            _allCharmsList.bindItem = (element, index) =>
            {
                Debug.Log($"Binding item at index: {index}");
                CharmAbilityBase charm = _charmManager.GetOwnedCharmAbilities()[index];
                if (charm != null)
                {
                    // Update the element with charm details
                    VisualElement imageSlot = element.Q<VisualElement>("ImageSlot");
                    imageSlot.RemoveFromClassList("empty-slot");
                    imageSlot.AddToClassList(charm._equipped ? "equipped-charm" : "charm-image");
                    imageSlot.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);
                    Debug.Log($"ImageSlot set to {imageSlot.style.backgroundImage}");
                    
                    
                    // Add hover behavior
                    element.RegisterCallback<MouseEnterEvent>(evt => DisplayCharmInfo(charm));
                    element.RegisterCallback<MouseLeaveEvent>(evt => ClearCharmInfo());

                    Button button = element.Q<Button>("Button");
                    button.clicked += () =>
                    {
                        if (charm._equipped)
                        {
                            UnequipCharm(charm);
                        }
                        else
                        {
                            EquipCharm(charm);
                        }
                    };
                }
                else
                {
                    // Empty slot (shouldn't normally happen)
                    VisualElement imageSlot = element.Q<VisualElement>("ImageSlot");
                    imageSlot.RemoveFromClassList("charm-image");
                    imageSlot.RemoveFromClassList("equipped-charm");
                    imageSlot.AddToClassList("empty-slot");

                    imageSlot.style.backgroundImage = null;

                    // Clear hover behavior
                    element.UnregisterCallback<MouseEnterEvent>(evt => { });
                    element.UnregisterCallback<MouseLeaveEvent>(evt => { });
                    
                    // Clear Button behavior
                    Button button = element.Q<Button>("Button");
                    button.clicked += () => { };
                }
            };
            
            if (_allCharmsList.itemsSource == null || _allCharmsList.itemsSource.Count == 0)
            {
                Debug.LogWarning("itemsSource is null or empty!");
            }
            else
            {
                Debug.Log("Rebuilding all charm list");
                Debug.Log($"AllCharmList item count: {_allCharmsList.itemsSource?.Count}");
                _allCharmsList.Rebuild();
            }
        }

        private void EquipCharm(CharmAbilityBase charm)
        {
            // 1. Check if the player has enough notches.
            if (_notchesUsed + charm.CharmInfo.EquipCost > _maxNotches)
            {
                Debug.Log("Not enough notches to equip this charm!");
                return;
            }

            // 2. Add the charm to the CharmManager's active charms list.
            _charmManager.ActivateOwnedCharmAbility(charm.CharmInfo.DisplayName);

            // 3. Refresh the UI
            RefreshUI();
        }

        private void UnequipCharm(CharmAbilityBase charm)
        {
            // 1. Remove the charm from the CharmManager's active charms list.
            _charmManager.DeactivateOwnedCharmAbility(charm.CharmInfo.DisplayName);

            // 2. Refresh the UI
            RefreshUI();
        }

        private void DisplayCharmInfo(CharmAbilityBase charm)
        {
            // Update the charm name
            _charmNameLabel.text = charm.CharmInfo.DisplayName;

            // Update the charm image
            if (charm.CharmInfo.Icon != null)
            {
                _charmImage.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);
            }

            // Update the charm description
            _charmDescriptionLabel.text = charm.CharmInfo.Description;
        }

        private void ClearCharmInfo()
        {
            _charmNameLabel.text = "";
            _charmImage.style.backgroundImage = null;
            _charmDescriptionLabel.text = "";
        }

        // Method to refresh the UI (call this if you change equipped/owned charms outside of this UI)
        public void RefreshUI()
        {
            _notchesUsed = _charmManager.GetActiveCharmAbilities().Sum(c => c.CharmInfo.EquipCost);
            _allCharmsList.Rebuild();
            InitializeEquippedCharmList();

        }
    }
}